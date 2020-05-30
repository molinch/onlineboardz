import { UserManager, WebStorageStateStore, Log } from "oidc-client";

class AuthenticationStore {
    constructor(onError) {
        this.onError = onError;

        this.userStore = new WebStorageStateStore({
          prefix: "oidc."
        });

        const config =  {
            authority: 'https://localhost:5000',
            client_id: 'js',
            redirect_uri: 'http://localhost:3000/login-callback',
            post_logout_redirect_uri: 'http://localhost:3000',
            response_type: 'code',
            scope: 'openid profile game-api',
            revokeAccessTokenOnSignout: true,
            userStore: this.userStore
        };

        Log.logger = console;
        this.manager = new UserManager(config);
    }
 
    isLoggedIn() {
        return this.user != null && this.user.access_token && !this.user.expired;
    }
 
    async getStateData(stateId) {
        try {
            return await this.userStore.get(stateId);
        } catch (error) {
            this.onError(error);
        }
    }

    async loadUser() {
        try {
            this.user = await this.manager.getUser();
        } catch (error) {
            this.onError(error);
        }
    }
 
    async login(data) {
        try {
            await this.manager.clearStaleState();
            await this.manager.signinRedirect(data);
        } catch (error) {
            this.onError(error);
        }
    }
 
    async completeLogin() {
        try {
            this.user = await this.manager.signinRedirectCallback();
        } catch (error) {
            this.onError(error);
        }
    }
 
    async logout() {
        try {
            await this.manager.signoutRedirect()
        } catch (error) {
            this.onError(error);
        }
    }
 
    async completeLogout() {
        try {
            await this.manager.signoutRedirectCallback();
            await this.manager.removeUser();
            this.user = null;
        } catch (error) {
            this.onError(error);
        }
    }
 }

 export default AuthenticationStore;