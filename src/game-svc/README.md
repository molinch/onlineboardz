# Game service database
MongoDB is used.
Robo 3T can be used to browse the data.

Create the user used by the service
```
db.createUser({user:"gamedb-user", pwd:"ComplicatedPassword", roles:[{role:"readWrite", db: "gamedb"}]});
```

The connection string should be placed in dotnet secrets:
```
dotnet user-secrets set "MongoConnectionString" "Host=localhost; user id=identityserver-db-user; password=password; database=identityserver"
```