using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirebaseRealtimeDatabaseService
{
	/// <summary>
	/// Firebase Realtime Database のレコードを取得・更新するサービス
	/// </summary>
	internal class FirebaseService : IFirebaseService
	{
		/// <summary>
		/// データベースのシークレット
		/// </summary>
		private string DatabaseSecret { get; }

		/// <summary>
		/// データベースのURL
		/// </summary>
		private string DatabaseUrl { get; }

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="databaseSecret">データベースのシークレット</param>
		/// <param name="databaseUrl">データベースのURL</param>
		internal FirebaseService(string databaseSecret, string databaseUrl)
		{
			DatabaseSecret = databaseSecret;
			DatabaseUrl = databaseUrl;
		}

		/// <summary>
		/// データベースのレコードを更新する(存在しない場合は登録する)
		/// </summary>
		/// <param name="path">レコードのパス</param>
		/// <param name="key">更新時にレコードを特定するキー</param>
		/// <param name="value">値</param>
		/// <returns>成功したか</returns>
		public async Task UpdateRecordAsync(string path, string key, object value)
		{
			// クエリを取得
			var query = GetDatabaseQuery(path);

			// データを登録
			await query.Child(key).PutAsync(value);
		}

		/// <summary>
		/// データベースの指定したキーのレコードを削除する
		/// </summary>
		/// <param name="path">パス</param>
		/// <param name="key">キー</param>
		/// <returns>削除されたか(もとから存在しなくてもtrue)</returns>
		public async Task DeleteRecordAsync(string path, string key)
		{
			// クエリを取得
			var query = GetDatabaseQuery(path);

			// データ削除(もともと存在しない場合は何もしない)
			await query.Child(key).DeleteAsync();

		}

		/// <summary>
		/// 指定したキーのレコードを1件取得する(なけれなnull)
		/// </summary>
		/// <typeparam name="T">レコードの型</typeparam>
		/// <param name="path">レコードのパス</param>
		/// <param name="key">レコードを特定するキー</param>
		/// <returns>対象レコード(なければnull)</returns>
		public async Task<T> GetRecordAsync<T>(string path, string key)
		{
			// クエリを取得
			var query = GetDatabaseQuery(path);

			// データを取得(存在しなければnullを返す)
			var record = await query.Child(key).OnceSingleAsync<T>();
			return record;
		}

		/// <summary>
		/// 指定したキーのレコード一覧をキーの昇順で取得する
		/// </summary>
		/// <typeparam name="T">レコードの型</typeparam>
		/// <param name="path">レコードのパス</param>
		/// <returns>対象レコード(なければnull)</returns>
		public async Task<IEnumerable<T>> GetRecordsAsync<T>(string path)
		{
			// クエリを取得
			var query = GetDatabaseQuery(path);

			// データを取得(存在しなければ0件のコレクションを返す)
			var record = await query.OrderByKey().OnceAsync<T>();
			return record.Select(r => r.Object);
		}

		/// <summary>
		/// 指定したパスのレコード一覧を順序を指定して取得する
		/// </summary>
		/// <typeparam name="T">レコードの型</typeparam>
		/// <param name="path">レコードのパス</param>
		/// <param name="propertyName">ソートするプロパティ名</param>
		/// <param name="limitCount">取得する件数</param>
		/// <param name="desc">降順で取得するか</param>
		/// <returns>対象レコード</returns>
		public async Task<IEnumerable<T>> GetRecordsOrderByAsync<T>(string path, string propertyName, int limitCount = 100, bool desc = false)
		{
			// クエリを取得
			var query = GetDatabaseQuery(path);

			// データを取得(存在しなければ0件のコレクションを返す)
			// ソートと件数指定は以下のURL参照
			// https://github.com/step-up-labs/firebase-database-dotnet
			if (desc == false)
			{
				var record = await query.OrderBy(propertyName).LimitToFirst(limitCount).OnceAsync<T>();
				return record.Select(r => r.Object);
			}
			else
			{
				var record = await query.OrderBy(propertyName).LimitToLast(limitCount).OnceAsync<T>();
				return record.Select(r => r.Object);
			}
		}

		/// <summary>
		/// 指定したパスのレコード一覧を「フィールド」の完全一致条件を指定して取得する
		/// (仕組み上、フィールドは部分一致で検索できない。部分一致で検索できるのはキーのみ)
		/// </summary>
		/// <typeparam name="T">レコードの型</typeparam>
		/// <param name="path">レコードのパス</param>
		/// <param name="propertyName">検索するプロパティ名</param>
		/// <param name="keyword">完全一致する文字列</param>
		/// <returns>対象レコード</returns>
		public async Task<List<T>> GetRecordsEqualToAsync<T>(string path, string propertyName, string keyword)
		{
			// クエリを取得
			var query = GetDatabaseQuery(path);

			// データを取得(存在しなければ0件のコレクションを返す)
			// フィルタの方法は以下URL参照
			// https://firebase.google.com/docs/database/admin/retrieve-data?hl=ja#range-queries
			var record = await query.OrderBy(propertyName).EqualTo(keyword).OnceAsync<T>();
			return record.Select(r => r.Object).ToList();
		}

		/// <summary>
		/// 指定したパスのレコード一覧を「キー」の前方一致条件を指定して取得する
		/// </summary>
		/// <typeparam name="T">レコードの型</typeparam>
		/// <param name="path">レコードのパス</param>
		/// <param name="key">検索するキー</param>
		/// <returns>対象レコード</returns>
		public async Task<IEnumerable<T>> GetRecordsStartAtAsync<T>(string path, string key)
		{
			// StartAtは、キーを文字列順に並び替えた状態で、指定した文字以降に位置するキーをすべて返す。
			// 従って、単純には前方一致はできない。詳細は以下参照。
			// https://firebase.google.com/docs/database/admin/retrieve-data?hl=ja#range-queries
			// よって、擬似的に前方一致を実現するために、「指定した文字」と「指定した文字＋最後の記号」で範囲取得する。
			// キーが英数字と記号のみを前提であれば、これで前方一致で取得できる。
			// 「|」は最後の記号ではないが、「}」エスケープ文字のため、安定して利用できる「|」を用いている。以下はコード表。
			// http://hp.vector.co.jp/authors/VA008536/data/ascii.html
			var query = GetDatabaseQuery(path);
			var record = await query.OrderByKey().StartAt(key).EndAt($"{key}|").OnceAsync<T>();
			return record.Select(r => r.Object);
		}

		/// <summary>
		/// データベースのクエリを取得
		/// </summary>
		/// <returns>設定値に応じたデータベースへの参照</returns>
		private ChildQuery GetDatabaseQuery(string path)
		{
			return new FirebaseClient(
					DatabaseUrl,
					new FirebaseOptions
					{
						AuthTokenAsyncFactory = () => Task.FromResult(DatabaseSecret),
					})
				.Child(path);
		}
	}
}
