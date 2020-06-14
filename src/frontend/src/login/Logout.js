import React, { useEffect } from 'react';
import { navigate } from '@reach/router';

const Logout = ({ authenticationStore, onError }) => {
    useEffect(() => {
        (async () => {
            try {
                await authenticationStore.completeLogout();
                navigate("/");
            } catch (error) {
                onError(error);
            }
        })();
    }, [authenticationStore, onError]);

    return (<></>);
};

export default Logout;