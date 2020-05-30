import React from 'react';
import { navigate } from "@reach/router"

class Logout extends React.Component {
    async componentDidMount() {
        try {
            await this.props.authenticationStore.completeLogout();
            navigate("/");
        } catch (error) {
            this.props.onError(error);
        }
    }

    render() {
        return (<div></div>);
    }
}

export default Logout;