# Slack channel
To go there: https://boardzproject.slack.com

# online boards
The goal is to have fun while making an online board games solution.


Frontend: React
Backend: .NET Core
DB: document DBs, so NoSQL (except for identity provider which will use PostgreSQL)

Patterns: "microservices" in the sense that each services holds his domain
Maybe these to start
- identity provider/account service: sign in (with google, fb, twitter, ms), sign-out, profile
- game service: where the logic of a game is, depending on the game complexity we may want != services
- chat service: chat friends, chat game opponents, global chat?

We can provide board games, card games, etc
It's better to start with some that are easy to implement like TicTacToe
