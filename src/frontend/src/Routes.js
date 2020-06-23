import React from 'react';
import { Router } from "@reach/router";
import Home from './Home';
import About from './About';
import Logout from './login/Logout';
import LoginCallback from './login/LoginCallback';
import LoginError from './login/LoginError';
import TicTacToeGame from './games/TicTacToe/TicTacToe';
import Play from './Play';

const Routes = ({ user, fetchWithUi, authenticationStore, onLoggingError, gameNotificationClient, gameReachabilityChecker, setError }) => (
    <Router>
        <Home path="/" />
        <About path="/about" />
        <Play
            path="/play"
            user={user}
            fetchWithUi={fetchWithUi}
            setError={setError}
        />
        <TicTacToeGame
            path="/games/TicTacToe/:gameId"
            user={user}
            fetchWithUi={fetchWithUi}
            gameNotificationClient={gameNotificationClient}
            gameReachabilityChecker={gameReachabilityChecker}
            setError={setError}
        />

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