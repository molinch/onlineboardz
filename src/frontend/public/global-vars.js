// is there a better way?
// the goal is to have that file modified just before server starts based on some environment variable data

function getGlobalVars() {
    switch (window.location.host) {
        case "boardz.fabien-molinet.fr":
            return {
                IdentityServerUri: "https://boardz-identity.fabien-molinet.fr",
                GameServiceUri: "https://boardz.fabien-molinet.fr/api/game-svc/",
            };
        default: // when running locally
            return {
                IdentityServerUri: "https://localhost:5000",
                GameServiceUri: "https://localhost:5001",
            };
    }
}

const globalVars = getGlobalVars()