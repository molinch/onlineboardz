import React from 'react';
import { Button } from 'antd';

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
                <Button onClick={() => this.props.login("google")}>Google</Button>
                <Button onClick={() => this.props.login("facebook")}>Facebook</Button>
            </div>
        );
    }
}

export default LoginStatus;