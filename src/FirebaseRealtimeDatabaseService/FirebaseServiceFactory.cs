namespace FirebaseRealtimeDatabaseService
{
	/// <summary>
	/// IFirebaseServiceを返すファクトリ
	/// </summary>
	public class FirebaseServiceFactory
	{
		/// <summary>
		/// IFirebaseServiceを返す
		/// </summary>
		/// <param name="databaseSecret">データベースのシークレット</param>
		/// <param name="databaseUrl">データベースのURL</param>
		/// <returns></returns>
		public IFirebaseService GetFirebaseService(string databaseSecret, string databaseUrl)
		{
			return new FirebaseService(databaseSecret, databaseUrl);
		}
	}
}