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
import missingProfilePicture from './missing-profile-picture.png';
import TicTacToe from './games/TicTacToe/TicTacToe';
import JoinGame from './JoinGame'
import Match from './Match'
import 'antd/dist/antd.css';
import { Layout, Menu } from 'antd';
import GameNotificationClient from './NotificationClient';
const { Header, Content, Footer } = Layout;
const { SubMenu } = Menu;

class App extends React.Component {
  constructor() {
      super();
      this.authenticationStore = new AuthenticationStore(this.onLoggingError);
      this.gameNotificationClient = new GameNotificationClient();
      this.state = { user: null };
  }

  onPlayerAdded() {

  }
  onGameStarted() {

  }

  async componentDidMount() {
    await this.tryGetUser();
  }

  async tryGetUser() {
    await this.authenticationStore.loadUser();
    const isLoggedIn = this.authenticationStore.isLoggedIn();
    const authUser = isLoggedIn ? this.authenticationStore.user : null;
    if (authUser != null) {
      this.setState({ user:
        {
          name: authUser.profile.name,
          email: authUser.profile.email,
          picture: authUser.profile.picture,
          getFetchOptions: function() {
            var accessToken = this.authenticationStore.user.access_token;
            return {
              headers: {
                Authorization: `Bearer ${accessToken}`
              }
            };
          }.bind(this)
        }
       });

      var accessToken = this.authenticationStore.user.access_token;
      await this.gameNotificationClient.load(accessToken);
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
    let accountMenu = (<></>);
    if (this.state.user) {
      const img = (<img src={this.state.user.picture || missingProfilePicture} alt="your avatar" style={{height: '50px'}} />);
      accountMenu = (
        <SubMenu key="account" title={img} style={{float: 'right'}}>
          <Menu.Item key="profile"><Link to="/profile">Profile</Link></Menu.Item>
          <Menu.Item key="friends"><Link to="/friends">Friends</Link></Menu.Item>
          <Menu.Item key="logout" onClick={this.logout.bind(this)}>Logout</Menu.Item>
        </SubMenu>
      );
    }

    return (
      <div>
        <Layout>
          <Header>
            <Menu theme="dark" mode="horizontal" defaultSelectedKeys={['home']}>
              <Menu.Item key="home"><Link to="/">Home</Link></Menu.Item>
              <Menu.Item key="about"><Link to="/about">About</Link></Menu.Item>
              <Menu.Item key="match"><Link to="/match">Match!</Link></Menu.Item>
              <Menu.Item key="join"><Link to="/join">Join game</Link></Menu.Item>
              <Menu.Item key="game/tictactoe"><Link to="/games/tictactoe">TicTacToe</Link></Menu.Item>
              {accountMenu}
            </Menu>
          </Header>
          <Content>
        
            <h1><img src={logo} className="App-logo" alt="Logo" />Online boardz</h1>
            
            <Router>
              <Home path="/" />
              <About path="/about" />
              <JoinGame path="/join" user={this.state.user} />
              <Match path="/match" />
              <TicTacToe path="/games/tictactoe" />

              <Login
                path="/login"
                authenticationStore={this.authenticationStore}
                onError={error => this.onLoggingError(error)}
              />
              <Logout
                path="/logout"
                authenticationStore={this.authenticationStore}
                onError={error => this.onLoggingError(error)}
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
          </Content>
          <Footer>
            Online boardz ©2020 to bunch of dawgz
          </Footer>
        </Layout>
      </div>
    );
  }
}

export default App;