import React, { useEffect } from 'react';

const LoginCallback = ({ authenticationStore, onError, user }) => {
    useEffect(() => {
        (async () => {
            if (user) return;

            try {
                await authenticationStore.completeLogin();
            } catch (error) {
                onError(error);
            }
        })();
    }, [authenticationStore, onError, user]);

    return (<div></div>);
}

export default LoginCallback;