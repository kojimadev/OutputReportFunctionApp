using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using FirebaseRealtimeDatabaseService;
using System.Threading.Tasks;

namespace OutputReportFunctionApp
{
    /// <summary>
    /// Azure Functions の関数を定義
    /// </summary>
    public class Functions
    {
        /// <summary>
        /// Firebase RealtimeDatabase の Secret
        /// </summary>
        private const string _DatabaseSecret = "";

        /// <summary>
        /// Firebase RealtimeDatabase の Url
        /// </summary>
        private const string _DatabaseUrl = "";

        /// <summary>
        /// Slackに通知する際に利用するIncoming WebhookのWebhook URL。Incoming Webhookの詳細は以下参照。
        /// https://slack.com/intl/ja-jp/help/articles/115005265063-Slack-%E3%81%A7%E3%81%AE-Incoming-Webhook-%E3%81%AE%E5%88%A9%E7%94%A8
        /// </summary>
        private const string _WebhookUrl = "Incoming WebhookのURL";

        /// <summary>
        /// 対象のユーザー名の一覧
        /// </summary>
        private static readonly List<string> _TargetUsers = new()
            {
                "Qiitaのユーザー名(例:kojimadev)",
            };

        /// <summary>
        /// OutputReportを月に1回、Slackに通知するAPI
        /// </summary>
        /// <param name="myTimer">第3引数が時刻で日本時間9時は標準時間0時、第4引数が日にちで1日を指定。</param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("OutputReport")]
        public static async Task OutputReport([TimerTrigger("0 0 0 1 * *")] TimerInfo myTimer, ILogger log)
        //public static async Task<IActionResult> OutputReport([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            // Qiita から投稿数とContributionsとフォロワー数を取得する
            var client = new HttpClient();

            var outputUsers = new List<OutputUser>();
            foreach (var userName in _TargetUsers)
            {
                // ユーザーページからデータを取得
                var response = await client.GetAsync("https://qiita.com/" + userName);
                var contents = await response.Content.ReadAsStringAsync();

                // 以下の形式で埋め込まれている投稿数を探して取得する
                // "articles":{"totalCount":59},
                const string articleStartString = "\"articles\":{\"totalCount\":";
                var articleStartIndex = contents.IndexOf(articleStartString);
                var articleSubstring = contents.Substring(articleStartIndex + articleStartString.Length);
                var articleStringLength = articleSubstring.IndexOf("}");
                var articleCountString = articleSubstring.Substring(0, articleStringLength);
                int articleCount = int.Parse(articleCountString);

                // "newContribution":14411,
                const string contributionStartString = "\"newContribution\":";
                var contributionStartIndex = contents.IndexOf(contributionStartString);
                var contributionSubstring = contents.Substring(contributionStartIndex + contributionStartString.Length);
                var contributionStringLength = contributionSubstring.IndexOf(",");
                var contributionsString = contributionSubstring.Substring(0, contributionStringLength);
                int contributions = int.Parse(contributionsString);

                // "followers":{"totalCount":841},
                const string followerStartString = "\"followers\":{\"totalCount\":";
                var followerStartIndex = contents.IndexOf(followerStartString);
                var followerSubstring = contents.Substring(followerStartIndex + followerStartString.Length);
                var followerStringLength = followerSubstring.IndexOf("}");
                var followerCountString = followerSubstring.Substring(0, followerStringLength);
                int followerCount = int.Parse(followerCountString);

                // Firebase から対象ユーザーのデータを取得する
                var firebaseService = GetFirebaseService();
                var outputUser = await firebaseService.GetRecordAsync<OutputUser>("outputUsers", userName);
                if (outputUser == null)
                {
                    // Firebase にデータが存在しなければ新規作成
                    outputUser = new OutputUser(userName);
                }
                // 今回の投稿数とContributionsとフォロワー数で対象ユーザーを更新して
                // 対象ユーザーに前回と今回の差分を保持させる
                outputUser.Update(articleCount, contributions, followerCount);

                // 更新後のデータで Firebase に登録しなおす
                await firebaseService.UpdateRecordAsync("outputUsers", userName, outputUser);

                // 最後に通知するために、ユーザー情報をリストに登録しておく
                outputUsers.Add(outputUser);
            }

            // 対象ユーザーが複数名の場合に、ユーザーごとの結果を1つの文字列に結合する
            var builder = new StringBuilder();
            foreach (var outputUser in outputUsers)
            {
                builder.AppendLine(outputUser.Report());
            }

            // 結合した文字列をSlackに投稿する
            var service = new SlackNotificationService();
            await service.Notify(builder.ToString(), _WebhookUrl, "OutputReport", "", false);

            //return new OkObjectResult(builder.ToString());
        }

        /// <summary>
        /// Firebaseのデータにアクセスするサービスを取得する
        /// </summary>
        /// <returns></returns>
        private static IFirebaseService GetFirebaseService()
        {
            var factory = new FirebaseServiceFactory();
            return factory.GetFirebaseService(_DatabaseSecret, _DatabaseUrl);
        }

    }
}
