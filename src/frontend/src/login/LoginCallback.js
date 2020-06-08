import React from 'react';
import { useRunOnce } from '../CustomHooks';

const LoginCallback = props => {
    useRunOnce(() => {
        (async () => {
            try {
                await props.authenticationStore.completeLogin();
            } catch (error) {
                props.onError(error);
            }
        })();
    });

    return (<div></div>);
}

export default LoginCallback;