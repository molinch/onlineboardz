import React from 'react';
import { Button } from 'antd';
import { useTranslation } from 'react-i18next';

const LoginStatus = ({ user, login }) => {
    const { t } = useTranslation();
    if (user) {
        return (
            <></>
        );
    }

    return (
        <div>
            {t('SignIn')}
            <Button onClick={() => login("google")}>Google</Button>
            <Button onClick={() => login("facebook")}>Facebook</Button>
        </div>
    );
}

export default LoginStatus;