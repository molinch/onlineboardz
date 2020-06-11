import React from 'react';
import { Router } from "@reach/router";
import Home from './Home';
import About from './About';
import Logout from './login/Logout';
import LoginCallback from './login/LoginCallback';
import LoginError from './login/LoginError';
import TicTacToe from './games/TicTacToe/TicTacToe';
import Play from './Play';
import PlayGame from './PlayGame'
import Match from './Match';

const Routes = props => (
    <Router>
        <Home path="/" />
        <About path="/about" />
        <Play path="/play" user={props.user} fetchWithUi={props.fetchWithUi} />
        <PlayGame path="/play/:gameName" user={props.user} fetchWithUi={props.fetchWithUi} />
        <TicTacToe path="/games/TicTacToe/:gameId" />

        <Logout
            path="/logout"
            authenticationStore={props.authenticationStore}
            onError={error => props.onLoggingError(error)}
        />
        <LoginCallback
            path="/login-callback"
            authenticationStore={props.authenticationStore}
            onError={error => props.onLoggingError(error)}
        />
        <LoginError path="/login-error" />
    </Router>
);

export default Routes;