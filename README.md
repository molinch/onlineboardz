# online boards
The goal is to have fun and learn while making an online board games solution.


Frontend: React

Backends: .NET Core

DB: NoSQL document DBs, (except for identity provider which will use PostgreSQL)

Patterns: "microservices" in the sense that each services holds his domain
- identity provider: sign in (with google, fb, twitter, ms), sign-out
- game service: where the logic of a game is, depending on the game complexity we may want != services
- account service: profile, friends, chat friends, chat game opponents, global chat?

We can provide board games, card games, etc
It's better to start with some that are easy to implement like TicTacToe
