# Game service
It uses .NET5, so to run you will need:
- SDK preview
- ASP.NET core runtime preview

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

# MongoDB support with .NET
The MongoDB driver is really limited...
Finding a way to transform trivial queries (for example that have aggregates) to C# turns out to be ridiculously complex and poorly documented.
Hence we also use MongoDB.Entities nuget which eases all this.
