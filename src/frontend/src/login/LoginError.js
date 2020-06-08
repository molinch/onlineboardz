import React, { useEffect, useState } from 'react';
import config from '../config';
import { useTranslation } from 'react-i18next';

const LoginError = () => {
    const { t } = useTranslation();
    const [errorText, setErrorText] = useState("fetching error details...");

    useEffect(() => {
        const getErrorText = async () => {
            const params = new URLSearchParams(window.location.search);
            const errorText = params.get("errorText");
            if (errorText) {
                return errorText;
            }
    
            const errorId = params.get("errorId");
            if (errorId) {
                try {
                    const response = await fetch(`${config.IdentityServerUri}/api/authenticate/error?errorid=${errorId}`, {
                        credentials: 'include'
                    });
                    return await response.text();
                } catch (error) {
                    console.log(error);
                    return error;
                }
            } else {
                return "no error found";
            }
        };

        (async () => {
            const errorText = await getErrorText();
            setErrorText(errorText); 
        })();
    }, []);

    return (
        <div>
            <h1>{t("OopsLoginError")}</h1>
            <div>{errorText}</div>
        </div>
    );
}

export default LoginError;