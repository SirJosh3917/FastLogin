using PlayerIOClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastLogin.Tester {
	class Program {
		static void Main(string[] args) {
			//benchmarks the speed

			List<Account> accs = new List<Account>() { //accounts
				new Account("//TODO:", "implement a bunch of accounts")
			};

			Console.WriteLine("Starting...");

			//login one just to make sure we dont affect benchmarks
			var cli = PlayerIOClient.PlayerIO.QuickConnect.SimpleConnect(FastLogin.EEGameId, accs[0].Email, accs[0].Password, null);
			cli.Multiplayer.CreateJoinRoom("PW", "Everybodyedits232", false, null, null).Disconnect();
			cli.Logout();

			Console.WriteLine("Running FastLogin");

			//create new fastlogin and add stuff
			var fs = new FastLogin();
			fs.AddAccountsToList(accs);
			
			Time("Fastlogin login time:",
				() => {
					var loggedInAccs = fs.LoginAndConnectAllAccounts( //login & connect each account
						(gameid, account) => PlayerIO.QuickConnect.SimpleConnect(gameid, account.Email, account.Password, null),
						(client) => client.Multiplayer.CreateJoinRoom("PW", "Everybodyedits232", false, null, null) //TODO: also add your OnMessage event handler here for super fast joining
					);
				}
			);

			var clientsCustomMethod = new List<Client>(); //declare vars
			var cons = new List<Connection>();

			Time("Normal login time:",
				() => {
					foreach (var i in accs) //time it this way
						clientsCustomMethod.Add(PlayerIO.QuickConnect.SimpleConnect(FastLogin.EEGameId, i.Email, i.Password, null));

					foreach (var i in clientsCustomMethod)
						cons.Add(i.Multiplayer.CreateJoinRoom("PW", "Everybodyedits232", false, null, null));
				}
			);

			Console.ReadLine();
		}

		public static void Time(string prefix, Action method, string suffix = "") {
			var stp = Stopwatch.StartNew();
			method();
			stp.Stop();
			Console.WriteLine($"{prefix}{stp.ElapsedMilliseconds}ms, {stp.ElapsedTicks} ticks");
		}
	}
}
