import React from 'react';
import { navigate } from "@reach/router"

class LoginCallback extends React.Component {
    async componentDidMount() {
        try {
            await this.props.authenticationStore.completeLogin();
            navigate("/");
        } catch (error) {
            this.props.onError(error);
        }
    }

    render() {
        return (<div></div>);
    }
}

export default LoginCallback;