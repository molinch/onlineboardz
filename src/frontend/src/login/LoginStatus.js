import React from 'react';

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
                <input type="button" onClick={() => this.props.login("google")} value="Google" />
                <input type="button" onClick={() => this.props.login("facebook")} value="Facebook" />
            </div>
        );
    }
}

export default LoginStatus;