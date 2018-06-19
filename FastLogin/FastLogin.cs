using PlayerIOClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastLogin {
	/// <summary>A thread-safe class to log multiple accounts very quickly.</summary>
	public class FastLogin {
		public const string EEGameId = "everybody-edits-su9rn58o40itdbnw69plyw";

		public FastLogin(string gameid = EEGameId) {
			this._alts = new List<Account>();
			this._gameid = gameid;
		}

		private object _listlocker = new object();
		private List<Account> _alts;
		private string _gameid;

		/// <summary>Adds multiple accounts to the list.</summary>
		/// <param name="e">The account to add.</param>
		public void AddAccountsToList(IEnumerable<Account> e) {
			if (e == null)
				throw new ArgumentNullException(nameof(e)); //whatever screw checking each one if it's null if it's null it's also your fault :p

			lock(this._listlocker) {
				this._alts.AddRange(e);
			}
		}

		/// <summary>Add an account</summary>
		/// <param name="e">The account to add</param>
		public void AddAccountToList(Account e) {
			if (e.Email == null)
				throw new ArgumentNullException(nameof(e.Email));

			if (e.Password == null)
				throw new ArgumentNullException(nameof(e.Password));

			lock(this._listlocker) { //thread safeness
				this._alts.Add(e);
			}
		}

		/// <summary>Logs in every account and then returns a list of the clients/connections to use.</summary>
		/// <returns>An array of all the clients that have been logged in</returns>
		public Client[] LoginAllAccounts(Func<string, Account, Client> loginCode) {
			var task = this.LoginAllAccountsAsync(loginCode);

			task.Wait();

			return task.Result;
		}

		/// <summary>Login each client asynchronously</summary>
		/// <returns>A task that will give all of the accounts</returns>
		public async Task<Client[]> LoginAllAccountsAsync(Func<string, Account, Client> loginCode) {
			var accountsToLogin = new List<Task<Client>>();

			lock (this._listlocker) {
				foreach (var i in this._alts)
					accountsToLogin.Add(Task.Run(() =>
						loginCode(this._gameid, i)
					));

				this._alts.Clear();
			}

			return await Task.WhenAll(accountsToLogin); //cannot await in the body of a lock statement /shrug
		}

		/// <summary>Logs in every client, and as soon as it's logged in, connects it using the connection code given.
		/// <remarks>You could attach OnMessage code as well as con.Send() code inside the connection code in order to speed up the joining process.</remarks></summary>
		/// <param name="connectionCode">The code to run as soon as a client connects.</param>
		/// <returns>A Tuple<Client, Connection>[] array of each client and their respective connection.</returns>
		public ConnectedAccount[] LoginAndConnectAllAccounts(Func<string, Account, Client> loginCode, Func<Client, Connection> connectionCode) {
			var task = this.LoginAndConnectAllAccountsAsync(loginCode, connectionCode);

			task.Wait();

			return task.Result;
		}

		/// <summary>Asynchronously logs in every client, and as soon as it's logged in, connects it using the connection code given.
		/// <remarks>You could attach OnMessage code as well as con.Send() code inside the connection code in order to speed up the joining process.</remarks></summary>
		/// <param name="connectionCode">The code to run as soon as a client connects.</param>
		/// <returns>A Tuple<Client, Connection>[] array of each client and their respective connection.</returns>
		public async Task<ConnectedAccount[]> LoginAndConnectAllAccountsAsync(Func<string, Account, Client> loginCode, Func<Client, Connection> connectionCode) {
			var accountsToLogin = new List<Task<ConnectedAccount>>();

			lock (this._listlocker) {
				foreach (var i in this._alts)
					accountsToLogin.Add(Task.Run(() => {
						var client = loginCode(this._gameid, i);
						return new ConnectedAccount(client, i.Auth, connectionCode(client));
					}));

				this._alts.Clear();
			}

			return await Task.WhenAll(accountsToLogin); //cannot await in the body of a lock statement /shrug
		}
	}

	//prevent System.ValueTuple from being a dependency ( available on nuget )
	public struct ConnectedAccount {
		public ConnectedAccount(Client client, AuthenticationType auth, Connection con) {
			this.Client = client;
			this.AuthenticationType = auth;
			this.Connection = con;
		}
		
		/// <summary>The client to use</summary>
		public Client Client { get; set; }

		/// <summary>The authentication type to use when logging in</summary>
		public AuthenticationType AuthenticationType { get; set; }

		/// <summary>The connection to use</summary>
		public Connection Connection { get; set; }
	}

	public struct Account {
		public Account(string email, string password, AuthenticationType auth = AuthenticationType.Simple) {
			this.Email = email;
			this.Password = password;
			this.Auth = auth;
		}

		public static Account New(string email, string password, AuthenticationType auth = AuthenticationType.Simple) =>
			new Account(email, password, auth);

		/// <summary>The email of an account</summary>
		public string Email { get; set; }

		/// <summary>The password of the account</summary>
		public string Password { get; set; }
		
		/// <summary>How the account should be authenticated</summary>
		public AuthenticationType Auth { get; set; }
	}

	public enum AuthenticationType {
		Simple = 0,
		Kongregate = 1,
		Facebook = 2,
		ArmorGames = 3,

		/// <summary>Special for EE only.</summary>
		Linked = 4,
	}
}
