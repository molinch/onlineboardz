import React from 'react';
import { Router, Link, navigate } from "@reach/router";
import Home from './Home';
import About from './About';
import LoginStatus from './login/LoginStatus';
import Login from './login/Login';
import Logout from './login/Logout';
import LoginCallback from './login/LoginCallback';
import LoginError from './login/LoginError';
import AuthenticationStore from './AuthenticationStore';
import './App.css';
import logo from './logo.svg';
import TicTacToe from './games/TicTacToe/TicTacToe';

class App extends React.Component {
  constructor() {
      super();
      this.authenticationStore = new AuthenticationStore(this.onLoggingError);
      this.state = { user: null };
  }

  async componentDidMount() {
    await this.tryGetUser();
  }

  async tryGetUser() {
    await this.authenticationStore.loadUser();
    const isLoggedIn = this.authenticationStore.isLoggedIn();
    const authUser = isLoggedIn ? this.authenticationStore.user : null;
    if (authUser != null) {
      const response = await fetch('https://localhost:5000/api/users/me', {
          credentials: 'include'
      });
      var user = await response.json();
      this.setState({ user: user });
    }
  }

  onLoggingError(error) {
    const errorText = encodeURIComponent(error);
    navigate("/login-error?errorText=" + errorText);
  }

  async login(provider) {
    await this.authenticationStore.login({
      data: { provider: provider }
    });
  }

  async logout() {
    if (this.state.user === null) return;
    this.setState({ user: null });
    await this.authenticationStore.logout();
  }

  render() {
    return (
      <div>
        <h1><img src={logo} className="App-logo" alt="Logo" />Online boardz</h1>
        <nav>
          <Link to="/">Home</Link>{" "}
          <Link to="/about">About</Link>
          <Link to="/games/tictactoe">TicTacToe</Link>
        </nav>
        
        <Router>
          <Home path="/" />
          <About path="/about" />
          <TicTacToe path="/games/tictactoe" />

          <Login
            path="/login"
            authenticationStore={this.authenticationStore}
          />
          <Logout
            path="/logout"
            authenticationStore={this.authenticationStore}
          />
          <LoginCallback
            path="/login-callback"
            authenticationStore={this.authenticationStore}
            onLoggedIn={async user => await this.tryGetUser()}
            onError={error => this.onLoggingError(error)}
          />
          <LoginError path="/login-error" />
        </Router>

        <LoginStatus
          user={this.state.user}
          login={provider => this.login(provider)}
          logout={() => this.logout()}
        />
      </div>
    );
  }
}

export default App;