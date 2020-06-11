import React, { useState } from 'react';
import { Link, navigate } from "@reach/router";
import LoginStatus from './login/LoginStatus';
import AuthenticationStore from './AuthenticationStore';
import './App.css';
import logo from './logo.svg';
import missingProfilePicture from './missing-profile-picture.png';
import 'antd/dist/antd.css';
import { Layout, Menu } from 'antd';
import GameNotificationClient from './NotificationClient';
import i18n from './languages/i18n';
import { useTranslation } from 'react-i18next';
import { useCreateOnce } from './CustomHooks';
import Routes from './Routes';
import FetchWithUIFeedback from './FetchWithUIFeedback';
const { Header, Content, Footer } = Layout;
const { SubMenu } = Menu;

function App() {
    const onLogged = async user => {
        if (window.location.href.includes("login") || window.location.href.includes("logout")) {
            navigate('/');
        }

        setUser({
            name: user.profile.name,
            email: user.profile.email,
            picture: user.profile.picture,
        });

        await gameNotificationClient.load(user.access_token);
    }

    const onLoggingError = error => {
        const errorText = encodeURIComponent(error);
        navigate("/login-error?errorText=" + errorText);
    }

    const login = async provider => {
        await authenticationStore.login({
            data: { provider: provider }
        });
    }

    const logout = async () => {
        if (!user) return;
        setUser(null);
        await authenticationStore.logout();
    }

    const [user, setUser] = useState(null);
    const { t } = useTranslation();
    const authenticationStore = useCreateOnce(() => new AuthenticationStore(onLoggingError, onLogged));
    const gameNotificationClient = useCreateOnce(() => new GameNotificationClient());
    const fetchWithUi = new FetchWithUIFeedback(() => authenticationStore?.user?.access_token);

    const switchLanguage = lng => {
        i18n.changeLanguage(lng);
    }

    const languageSelector = (
        <SubMenu key="language" title={t("Languages")} style={{ float: 'right' }}>
            <Menu.Item key="language-fr" onClick={() => switchLanguage('fr')}>Français</Menu.Item>
            <Menu.Item key="language-en" onClick={() => switchLanguage('en')}>English</Menu.Item>
        </SubMenu>
    );
    let accountMenu = (<></>);
    if (user) {
        const img = (
        <img
            src={user.picture || missingProfilePicture}
            alt={t("YourAvatar")} style={{ height: '50px' }} referrerPolicy="no-referrer"
        />);
        accountMenu = (
            <SubMenu key="account" title={img} style={{ float: 'right' }}>
                <Menu.Item key="profile"><Link to="/profile">{t("Profile")}</Link></Menu.Item>
                <Menu.Item key="friends"><Link to="/friends">{t("Friends")}</Link></Menu.Item>
                <Menu.Item key="logout" onClick={logout}>{t("Logout")}</Menu.Item>
            </SubMenu>
        );
    }

    return (
        <div>
            <Layout>
                <Header>
                    <Menu theme="dark" mode="horizontal" defaultSelectedKeys={['home']}>
                        <Menu.Item key="home"><Link to="/">{t("Home")}</Link></Menu.Item>
                        <Menu.Item key="about"><Link to="/about">{t("About")}</Link></Menu.Item>
                        <Menu.Item key="play"><Link to="/play">{t("Play")}</Link></Menu.Item>
                        <Menu.Item key="games-TicTacToe"><Link to="/games/TicTacToe/dummy">{t("TicTacToe")}</Link></Menu.Item>
                        {languageSelector}
                        {accountMenu}
                    </Menu>
                </Header>
                <Content>
                    <h1><img src={logo} className="App-logo" alt={t("OnlineBoardzLogo")} />{t("OnlineBoardz")}</h1>

                    <Routes
                        user={user}
                        fetchWithUi={fetchWithUi}
                        authenticationStore={authenticationStore}
                        onLoggingError={onLoggingError}
                    />

                    <LoginStatus
                        user={user}
                        login={provider => login(provider)}
                    />
                </Content>
                <Footer>
                    {t("Footer")}
                </Footer>
            </Layout>
        </div>
    );
}

export default App;