// maybe there are libs doing that, but it's really not much so why bother?
// if it grows we can reconsider having a dependency

class AuthenticatedFetch {
    constructor(getAccessToken) {
        this._getAccessToken = getAccessToken;
    }

    getDefaultOptions = () => {
        const accessToken = this._getAccessToken();
        return {
            headers: {
                Authorization: `Bearer ${accessToken}`
            }
        };
    }

    get = (uri, errorHandler) => {
        const options = this.getDefaultOptions();
        return this._withErrorHandling(fetch(uri, options), errorHandler);
    }

    post = (uri, data, errorHandler) => {
        const options = this.getDefaultOptions();
        options.method = 'POST';
        options.headers['content-type'] = 'application/json';
        options.body = JSON.stringify(data);
        return this._withErrorHandling(fetch(uri, options), errorHandler);
    }

    patch = (uri, data, errorHandler) => {
        const options = this.getDefaultOptions();
        options.method = 'PATCH';
        options.headers['content-type'] = 'application/json';
        options.body = JSON.stringify(data);
        return this._withErrorHandling(fetch(uri, options), errorHandler);
    }

    put = (uri, data, errorHandler) => {
        const options = this.getDefaultOptions();
        options.method = 'PATCH';
        options.headers['content-type'] = 'application/json';
        options.body = JSON.stringify(data);
        return this._withErrorHandling(fetch(uri, options), errorHandler);
    }

    delete = (uri, errorHandler) => {
        const options = this.getDefaultOptions();
        options.method = 'DELETE';
        return this._withErrorHandling(fetch(uri, options), errorHandler);
    }

    _withErrorHandling = async (fetchCall, errorHandler) => {
        try {
            const response = await fetchCall;
            const status = response.status;
            const result = await response.json();
            if (status >= 200 && status < 300) {
                return result;
            } if (result.title && status >= 400 && status < 500) {
                console.log('A client error occured while fetching');
                console.log(result);
                return errorHandler.validation(result.title);
            } else if (status >= 500) {
                console.log('A server error occured while fetching');
                console.log(result);
                return errorHandler.server();
            } else {
                console.log('An unknown result has been returned while fetching');
                console.log(result);
                return errorHandler.general(Error("Unknown fetch result"));
            }
        } catch (error) {
            console.log('A general error occured while fetching');
            console.log(error);
            return errorHandler.general(error);
        }
    }
}

export default AuthenticatedFetch;