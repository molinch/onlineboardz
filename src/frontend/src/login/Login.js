import React from 'react';
import { navigate } from "@reach/router"

class Login extends React.Component {
    async componentDidMount() {
        const returnUrl = new URLSearchParams(window.location.search).get("ReturnUrl");
        if (!returnUrl) {
            const errorText = encodeURIComponent("missing return URL");
            navigate("/login-error?errorText=" + errorText);
        }

        const params = new URLSearchParams(returnUrl);
        const stateId = params.get("state");
        if (stateId) {
            try {
                const stateData = await this.props.authenticationStore.getStateData(stateId);
                if (stateData)
                {
                    const provider = JSON.parse(stateData).data.provider;
                    document.location.href = `https://localhost:5000/api/authenticate/${provider}?returnUrl=${encodeURIComponent(returnUrl)}`;
                    return;
                }
            } catch (error) {
                this.props.onError(error);
                return;
            }
        }

        navigate("/");
    }

    render() {
        return (<div></div>);
    }
}

export default Login;