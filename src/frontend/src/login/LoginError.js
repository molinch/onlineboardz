import React from 'react';

class LoginError extends React.Component {
    constructor() {
        super();
        this.state = { errorText: "fetching error details..." };
    }

    async componentDidMount() {
        const errorText = await this.getErrorText();
        this.setState({ errorText: errorText });
    }

    async getErrorText() {
        const params = new URLSearchParams(window.location.search);
        const errorText = params.get("errorText");
        if (errorText) {
            return errorText;
        }

        const errorId = params.get("errorId");
        if (errorId) {
            try {
                const response = await fetch('https://localhost:5000/api/authenticate/error?errorid=' + errorId, {
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
    }

    render() {
        return (
            <div>
                <h1>Oops an error occured during login...</h1>
                <div>{this.state.errorText}</div>
            </div>
        );
    }
}

export default LoginError;