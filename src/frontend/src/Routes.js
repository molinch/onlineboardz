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

const Routes = ({ user, fetchWithUi, authenticationStore, onLoggingError }) => (
    <Router>
        <Home path="/" />
        <About path="/about" />
        <Play path="/play" user={user} fetchWithUi={fetchWithUi} />
        <PlayGame path="/play/:gameName" user={user} fetchWithUi={fetchWithUi} />
        <TicTacToe path="/games/TicTacToe/:gameId" />

        <Logout
            path="/logout"
            authenticationStore={authenticationStore}
            onError={error => onLoggingError(error)}
        />
        <LoginCallback
            path="/login-callback"
            authenticationStore={authenticationStore}
            onError={error => onLoggingError(error)}
            user={user}
        />
        <LoginError path="/login-error" />
    </Router>
);

export default Routes;