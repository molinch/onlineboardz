// is there a better way?
// the goal is to have that file modified just before server starts based on some environment variable data

if (window.location.host === "boardz.fabien-molinet.fr") {
    const globalVars = {
        IdentityServerUri: "https://boardz.fabien-molinet.fr/api/identity-server/",
        GameServiceUri: "https://boardz.fabien-molinet.fr/api/game-svc/",
    };
} else {
    const globalVars = {
        IdentityServerUri: "https://localhost:5000",
        GameServiceUri: "https://localhost:5001",
    };
}