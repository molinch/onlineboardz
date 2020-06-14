import React, { useEffect } from 'react';

const LoginCallback = ({ authenticationStore, onError }) => {
    useEffect(() => {
        (async () => {
            debugger;
            try {
                await authenticationStore.completeLogin();
            } catch (error) {
                onError(error);
            }
        })();
    }, [authenticationStore, onError]);

    return (<div></div>);
}

export default LoginCallback;