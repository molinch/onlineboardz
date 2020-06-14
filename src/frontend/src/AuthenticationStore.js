import { UserManager, WebStorageStateStore, Log } from 'oidc-client';
import config from './config';

class AuthenticationStore {
    constructor(onError, onLogged) {
        this.onError = onError;

        this.userStore = new WebStorageStateStore({
          prefix: "oidc."
        });

        this.managerConfig =  {
            authority: config.IdentityServerUri,
            client_id: 'js',
            redirect_uri: `${window.location.protocol}//${window.location.host}/login-callback`,
            response_type: 'code',
            scope: 'openid profile game-api offline_access',
            revokeAccessTokenOnSignout: true,
            automaticSilentRenew: true,
            silent_redirect_uri: `${window.location.protocol}//${window.location.host}/silent_renew.html`,
            userStore: this.userStore,
            loadUserInfo: false
        };

        Log.logger = console;
        Log.level = Log.DEBUG;
        this.manager = new UserManager(this.managerConfig);
        this.onLogged = onLogged;

        (async () => {
            await this.loadUser();
            if (this.isLoggedIn()) {
                // access token is still valid
                onLogged(this.user);
            } else {
                if (!this.user) {
                    // no identity information found in localstorage
                    return;
                }

                // access token is expired, try getting a new one using the refresh token
                try {
                    this.user = await this.manager.signinSilent();
                    if (this.isLoggedIn) {
                        onLogged(this.user);
                    }
                } catch (error) {
                    if (error.message === "login_required") {
                        console.log("user is not logged");
                    } else {
                        console.log(error);
                    }
                }
            }
        })();
    }
 
    isLoggedIn = () => {
        return this.user != null && this.user.access_token && !this.user.expired;
    }
 
    getStateData = async stateId => {
        try {
            return await this.userStore.get(stateId);
        } catch (error) {
            this.onError(error);
        }
    }

    loadUser = async () => {
        try {
            this.user = await this.manager.getUser();
        } catch (error) {
            this.onError(error);
        }
    }
 
    login = async data => {
        try {
            await this.manager.clearStaleState();
            await this.manager.signinRedirect(data);
        } catch (error) {
            this.onError(error);
        }
    }
 
    completeLogin = async () => {
        try {
            this.user = await this.manager.signinRedirectCallback();

            if (this.isLoggedIn()) {
                this.onLogged(this.user);
            }
        } catch (error) {
            debugger;
            this.onError(error);
        }
    }
 
    logout = async () => {
        try {
            await this.manager.signoutRedirect()
        } catch (error) {
            this.onError(error);
        }
    }
 }

 export default AuthenticationStore;