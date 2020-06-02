import React from 'react';
import { Button } from 'antd';
import { GoogleFilled, FacebookFilled } from '@ant-design/icons';

class LoginStatus extends React.Component {
    render() {
        const isLoginCallback = document.location.pathname === "/login-callback";
        if (isLoginCallback) return (<div></div>);

        const user = this.props.user;
        if (user) {
            return (
                <div>
                    <strong>Logged in! <span role="img" aria-label="party">🎉</span></strong><br />
                    <div>Welcome {user.name}</div>
                </div>
            );
        }

        return (
            <div>
                Sign-in
                <Button icon={<GoogleFilled />} onClick={() => this.props.login("google")}>Google</Button>
                <Button icon={<FacebookFilled />} onClick={() => this.props.login("facebook")}>Facebook</Button>
            </div>
        );
    }
}

export default LoginStatus;