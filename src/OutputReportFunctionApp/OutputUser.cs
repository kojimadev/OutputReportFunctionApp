using System;
using System.Collections.Generic;
using System.Text;

namespace OutputReportFunctionApp
{
    /// <summary>
    /// OutputReportに利用するユーザー情報
    /// </summary>
    class OutputUser
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="qiitaUserName">Qiitaのユーザー名</param>
        public OutputUser(string qiitaUserName)
        {
            QiitaUserName = qiitaUserName;
        }

        /// <summary>
        /// Qiitaのユーザー名
        /// </summary>
        public string QiitaUserName { get; set; }

        /// <summary>
        /// Qiitaの投稿数
        /// </summary>
        public int QiitaArticleCount { get; set; }

        /// <summary>
        /// 前回のQiitaの投稿数
        /// </summary>
        public int PreviousQiitaArticleCount { get; set; }

        /// <summary>
        /// QiitaのContributors
        /// </summary>
        public int QiitaContributions { get; set; }

        /// <summary>
        /// 前回のQiitaのContributors
        /// </summary>
        public int PreviousQiitaContributions { get; set; }

        /// <summary>
        /// Qiitaのフォロワー数
        /// </summary>
        public int QiitaFollowers { get; set; }

        /// <summary>
        /// 前回のQiitaのフォロワー数
        /// </summary>
        public int PreviousQiitaFollowers { get; set; }

        /// <summary>
        /// 新しいデータに更新する
        /// </summary>
        /// <param name="qiitaArticleCount">Qiitaの投稿数</param>
        /// <param name="qiitaContributions">QiitaのContributions</param>
        /// <param name="qiitaFollowers">Qiitaのフォロワー</param>
        public void Update(int qiitaArticleCount, int qiitaContributions, int qiitaFollowers)
        {
            PreviousQiitaArticleCount = QiitaArticleCount;
            PreviousQiitaContributions = QiitaContributions;
            PreviousQiitaFollowers = QiitaFollowers;
            QiitaArticleCount = qiitaArticleCount;
            QiitaContributions = qiitaContributions;
            QiitaFollowers = qiitaFollowers;
        }

        /// <summary>
        /// 対象ユーザーのアウトプット情報を出力する
        /// </summary>
        /// <returns></returns>
        public string Report()
        {
            return $"{QiitaUserName}{Environment.NewLine}" +
                $"Qiita 投稿数：{QiitaArticleCount} (＋{QiitaArticleCount - PreviousQiitaArticleCount}){Environment.NewLine}" +
                $"Qiita Contributions：{QiitaContributions} (＋{QiitaContributions - PreviousQiitaContributions}){Environment.NewLine}" +
                $"Qiita フォロワー：{QiitaFollowers} (＋{QiitaFollowers - PreviousQiitaFollowers}){Environment.NewLine}" +
                $"------------------------------------------";
        }
    }
}
