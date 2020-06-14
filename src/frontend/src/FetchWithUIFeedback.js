// maybe there are libs doing that, but it's really not much so why bother?
// if it grows we can reconsider having a dependency

import React from 'react';
import { Alert } from 'antd';
import AuthenticatedFetch from './AuthenticatedFetch';

class FetchWithUIFeedback {
    constructor(getAccessToken) {
        this._authFetch = new AuthenticatedFetch(getAccessToken);
        this._errorCount = 0;
        this._errorHandler = {
            validation: title => ({
                error: (
                    <Alert key={`warning-${this._errorCount++}`}
                        message="Warning" type="warning" showIcon closable
                        description={title} 
                    />
                )
            }),
            server: () => ({
                error: (
                    <Alert key={`server-error-${this._errorCount++}`}
                        message="Error" type="error" showIcon closable 
                        description="A server error occured please try again later"
                    />
                )
            }),
            general: () => ({
                error: (
                    <Alert key={`general-error-${this._errorCount++}`}
                        message="Error" type="error" showIcon closable
                        description="An error occured, make sure you are connected to network/internet"
                    />
                )
            }),
        };
    }

    get = uri => this._authFetch.get(uri, this._errorHandler)

    post = (uri, data) => this._authFetch.post(uri, data, this._errorHandler)

    patch = (uri, data) => this._authFetch.patch(uri, data, this._errorHandler)

    put = (uri, data) => this._authFetch.put(uri, data, this._errorHandler)

    delete = uri => this._authFetch.post(uri, this._errorHandler)
}

export default FetchWithUIFeedback;