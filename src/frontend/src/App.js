import React from 'react';
import { Router, Link, navigate } from "@reach/router";
import Home from './Home';
import About from './About';
import LoginStatus from './login/LoginStatus';
import Logout from './login/Logout';
import LoginCallback from './login/LoginCallback';
import LoginError from './login/LoginError';
import AuthenticationStore from './AuthenticationStore';
import './App.css';
import logo from './logo.svg';
import missingProfilePicture from './missing-profile-picture.png';
import TicTacToe from './games/TicTacToe/TicTacToe';
import Play from './Play'
import Match from './Match'
import 'antd/dist/antd.css';
import { Layout, Menu } from 'antd';
import GameNotificationClient from './NotificationClient';
const { Header, Content, Footer } = Layout;
const { SubMenu } = Menu;

class App extends React.Component {
  constructor() {
      super();
      this.authenticationStore = new AuthenticationStore(this.onLoggingError, this.onLogged.bind(this));
      this.gameNotificationClient = new GameNotificationClient();
      this.state = { user: null };
  }

  onPlayerAdded() {

  }
  onGameStarted() {

  }

  async onLogged(user) {
    this.setState({ user:
      {
        name: user.profile.name,
        email: user.profile.email,
        picture: user.profile.picture,
        getFetchOptions: function() {
          var accessToken = this.authenticationStore.user.access_token;
          return {
            headers: {
              Authorization: `Bearer ${accessToken}`
            }
          };
        }
      }
     });

    await this.gameNotificationClient.load(user.access_token);
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
              <Menu.Item key="play"><Link to="/play">Play!</Link></Menu.Item>
              <Menu.Item key="game/tictactoe"><Link to="/games/tictactoe">TicTacToe</Link></Menu.Item>
              {accountMenu}
            </Menu>
          </Header>
          <Content>
        
            <h1><img src={logo} className="App-logo" alt="Logo" />Online boardz</h1>
            
            <Router>
              <Home path="/" />
              <About path="/about" />
              <Play path="/play" user={this.state.user} />
              <TicTacToe path="/games/tictactoe" />

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