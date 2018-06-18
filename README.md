# FastLogin
Automatically login a bunch of accounts to PlayerIO with extreme haste.

```
PM> Install-Package PlayerIOClient-FastLogin
```

## Benchmarks?

With 5 accounts:

```
Fastlogin login time:	1035ms, 4058759 ticks
Normal login time:	2285ms, 8959369 ticks
```

With 4 accounts:

```
Fastlogin login time:	1217ms, 4771013 ticks
Normal login time:	1809ms, 7093238 ticks
```

With 3 accounts:

```
Fastlogin login time:	728ms, 2854813 ticks
Normal login time:	1290ms, 5057036 ticks
```

With 2 accounts:

```
Fastlogin login time:	662ms, 2597472 ticks
Normal login time:	1077ms, 4221361 ticks
```

With 1 account:

```
Fastlogin login time:	474ms, 1860820 ticks
Normal login time:	433ms, 1698411 ticks
```

## What does it do?

FastLogin uses asynchronous methods to quickly login each account, as well as connect them each to a world.
By using asynchronous methods, it is able to leave it's competition in the dust in terms of login time.

## Mutliple authentication methods? CUSTOM authentication methods?

FastLogin uses Func<> parameters to ensure that YOU, the user, have maximum control over FastLogin.
FastLogin is NOT game specific, so you can use it in any PlayerIO game you desire!

## How do I use this?

```
var fs = new FastLogin();
```

FastLogin, by default, uses EE's GameId but you can specify a different one

```
var fs = new FastLogn("blockworks-frdrlhtjneoipehnx9tmg");
```

Next, add the accounts you want to use.

```
fs.AddAccountToList(new Account("email", "password", AuthenticationType.Simple));
//There is an AddAccounts method.
```

After adding all the accounts, connect them all!

```
var accs = fs.LoginAndConnectAllAccounts(
	(gameid, account) => PlayerIO.QuickConnect.SimpleConnect(gameid, account.Email, account.Password, null),
	(client) => client.Multiplayer.CreateJoinRoom("PW", "Everybodyedits232", false, null, null) //TODO: add custom OnMessage within this lambda expression
);

//alternatively, use the ( not recommended ) LoginAllAccounts function for just Client[]s

var accs = fs.LoginAllAccounts(
	(gameid, account) => PlayerIO.QuickConnect.SimpleConnect(gameid, account.Email, account.Password, null),
);
```

And you're done!