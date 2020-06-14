import React from 'react';
import { Button } from 'antd';
import { Trans, useTranslation } from 'react-i18next';

const LoginStatus = ({ user, login }) => {
    const { t } = useTranslation();
    if (user) {
        return (
            <div className="loggedIn">
                <strong>{t("LoggedIn")} <span role="img" aria-label="party">🎉</span></strong><br />
                <div>
                    <Trans i18nKey="WelcomeUser">
                        Welcome {{userName: user.name}}
                    </Trans>
                </div>
            </div>
        );
    }

    return (
        <div>
            Sign-in
            <Button onClick={() => login("google")}>Google</Button>
            <Button onClick={() => login("facebook")}>Facebook</Button>
        </div>
    );
}

export default LoginStatus;