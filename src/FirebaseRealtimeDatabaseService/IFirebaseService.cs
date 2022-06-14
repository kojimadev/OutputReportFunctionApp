using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FirebaseRealtimeDatabaseService
{
	/// <summary>
	/// Firebase Realtime Database のレコードを取得・更新するインターフェイス
	/// </summary>
	public interface IFirebaseService
	{
		/// <summary>
		/// データベースのレコードを更新する(存在しない場合は登録する)
		/// </summary>
		/// <param name="path">レコードのパス</param>
		/// <param name="key">更新時にレコードを特定するキー</param>
		/// <param name="value">値</param>
		/// <returns>成功したか</returns>
		Task UpdateRecordAsync(string path, string key, object value);

		/// <summary>
		/// データベースの指定したキーのレコードを削除する
		/// </summary>
		/// <param name="path">パス</param>
		/// <param name="key">キー</param>
		/// <returns>削除されたか(もとから存在しなくてもtrue)</returns>
		Task DeleteRecordAsync(string path, string key);

		/// <summary>
		/// 指定したキーのレコードを1件取得する(なけれなnull)
		/// </summary>
		/// <typeparam name="T">レコードの型</typeparam>
		/// <param name="path">レコードのパス</param>
		/// <param name="key">レコードを特定するキー</param>
		/// <returns>対象レコード(なければnull)</returns>
		Task<T> GetRecordAsync<T>(string path, string key);

		/// <summary>
		/// 指定したキーのレコード一覧をキーの順序で取得する
		/// </summary>
		/// <typeparam name="T">レコードの型</typeparam>
		/// <param name="path">レコードのパス</param>
		/// <returns>対象レコード(なければnull)</returns>
		Task<IEnumerable<T>> GetRecordsAsync<T>(string path);

		/// <summary>
		/// 指定したパスのレコード一覧を順序を指定して取得する
		/// </summary>
		/// <typeparam name="T">レコードの型</typeparam>
		/// <param name="path">レコードのパス</param>
		/// <param name="propertyName">ソートするプロパティ名</param>
		/// <param name="limitCount">取得する件数</param>
		/// <param name="desc">降順で取得するか</param>
		/// <returns>対象レコード</returns>
		Task<IEnumerable<T>> GetRecordsOrderByAsync<T>(string path, string propertyName, int limitCount = 100, bool desc = false);

		/// <summary>
		/// 指定したパスのレコード一覧を「フィールド」の完全一致条件を指定して取得する
		/// (仕組み上、フィールドは部分一致で検索できない。部分一致で検索できるのはキーのみ)
		/// </summary>
		/// <typeparam name="T">レコードの型</typeparam>
		/// <param name="path">レコードのパス</param>
		/// <param name="propertyName">検索するプロパティ名</param>
		/// <param name="keyword">完全一致する文字列</param>
		/// <returns>対象レコード</returns>
		Task<List<T>> GetRecordsEqualToAsync<T>(string path, string propertyName, string keyword);

		/// <summary>
		/// 指定したパスのレコード一覧を「キー」の前方一致条件を指定して取得する
		/// </summary>
		/// <typeparam name="T">レコードの型</typeparam>
		/// <param name="path">レコードのパス</param>
		/// <param name="key">検索するキー</param>
		/// <returns>対象レコード</returns>
		Task<IEnumerable<T>> GetRecordsStartAtAsync<T>(string path, string key);

	}
}
