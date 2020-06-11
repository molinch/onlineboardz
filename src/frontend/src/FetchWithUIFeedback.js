// maybe there are libs doing that, but it's really not much so why bother?
// if it grows we can reconsider having a dependency

import React from 'react';
import { Alert } from 'antd';
import AuthenticatedFetch from './AuthenticatedFetch';

class FetchWithUIFeedback {
    constructor(getAccessToken) {
        this.authFetch = new AuthenticatedFetch(getAccessToken);
        this.errorCount = 0;
        this.errorHandler = {
            validation: title => ({
                error: (
                    <Alert key={`warning-${this.errorCount++}`}
                        message="Warning" type="warning" showIcon closable
                        description={title} 
                    />
                )
            }),
            server: () => ({
                error: (
                    <Alert key={`server-error-${this.errorCount++}`}
                        message="Error" type="error" showIcon closable 
                        description="A server error occured please try again later"
                    />
                )
            }),
            general: () => ({
                error: (
                    <Alert key={`general-error-${this.errorCount++}`}
                        message="Error" type="error" showIcon closable
                        description="An error occured, make sure you are connected to network/internet"
                    />
                )
            }),
        };
    }

    get = uri => this.authFetch.get(uri, this.errorHandler)

    post = (uri, data) => this.authFetch.post(uri, data, this.errorHandler)

    patch = (uri, data) => this.authFetch.patch(uri, data, this.errorHandler)

    put = (uri, data) => this.authFetch.put(uri, data, this.errorHandler)

    delete = uri => this.authFetch.post(uri, this.errorHandler)
}

export default FetchWithUIFeedback;