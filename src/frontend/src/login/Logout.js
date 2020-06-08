import React from 'react';
import { navigate } from '@reach/router';
import { useRunOnce } from '../CustomHooks';

const Logout = props => {
    useRunOnce(() => {
        (async () => {
            try {
                await props.authenticationStore.completeLogout();
                navigate("/");
            } catch (error) {
                props.onError(error);
            }
        })();
    });

    return (<></>);
};

export default Logout;