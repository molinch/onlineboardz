import React from 'react';
import { navigate } from "@reach/router"

class Logout extends React.Component {
    async componentDidMount() {
        await this.props.authenticationStore.completeLogout();
        navigate("/");
    }

    render() {
        return (<div></div>);
    }
}

export default Logout;