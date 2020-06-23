import React, { useState, useCallback, useEffect } from 'react';
import { Link, navigate, useLocation } from "@reach/router";
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
import ReachabilityChecker from './ReachabilityChecker';
import config from './config';
import { GameTypeInfo } from './games/GameType';
const { Header, Content, Footer, Sider } = Layout;
const { SubMenu } = Menu;

const onLoggingError = error => {
    const errorText = encodeURIComponent(error);
    navigate("/login-error?errorText=" + errorText);
}

const gameNotificationClient = new GameNotificationClient();
const gameReachabilityChecker = new ReachabilityChecker(`${config.GameServiceUri}/reachability`);

function App() {
    console.log('Render App');
    const [user, setUser] = useState(null);
    const [menuItems, setMenuItems] = useState(<></>);
    const [menuSelected, setMenuSelected] = useState("/home");
    const [error, setError] = useState(null);
    const [myGames, setMyGames] = useState(null);
    const { t } = useTranslation();
    const location = useLocation();

    const authenticationStore = useCreateOnce(() => new AuthenticationStore(
        onLoggingError,

        async user => {
            if (window.location.href.includes("login") || window.location.href.includes("logout")) {
                navigate('/');
            }

            setUser({
                id: user.profile.sub,
                name: user.profile.name,
                email: user.profile.email,
                picture: user.profile.picture,
            });

            gameNotificationClient.load(user.access_token);

            (async () => {
                const fetchedGames = await fetchWithUi.get(`${config.GameServiceUri}/games/mine`);
                if (fetchedGames.error) {
                    setError(fetchedGames.error);
                    return;
                }
                setMyGames(fetchedGames);
            })();
        }
    ));

    const login = useCallback(async provider => {
        await authenticationStore.login({
            data: { provider: provider }
        });
    }, [authenticationStore]);

    const logout = useCallback(async () => {
        if (!user) return;
        setUser(null);
        await authenticationStore.logout();
    }, [user, authenticationStore]);

    const fetchWithUi = useCreateOnce(() => new FetchWithUIFeedback(() => authenticationStore?.user?.access_token));

    const switchLanguage = lng => {
        i18n.changeLanguage(lng);
    };

    const pathName = location.pathname;
    useEffect(() => {
        const languageSelector = (
            <SubMenu key="language" title={t("Languages")}>
                <Menu.Item key="language-fr" onClick={() => { switchLanguage('fr'); return false; }}>Français</Menu.Item>
                <Menu.Item key="language-en" onClick={() => switchLanguage('en')}>English</Menu.Item>
            </SubMenu>
        );
        let accountMenu = (<></>);
        let myGamesMenu = (<></>);
        if (user) {
            const img = (
                <div>
                    <img
                        src={user.picture || missingProfilePicture}
                        alt={t("YourAvatar")} style={{ height: '50px' }} referrerPolicy="no-referrer"
                    />
                    <span className="userName">{user.name}</span>
                </div>
            );
            accountMenu = (
                <SubMenu key="account" title={img}>
                    <Menu.Item key="/profile"><Link to="/profile">{t("Profile")}</Link></Menu.Item>
                    <Menu.Item key="/friends"><Link to="/friends">{t("Friends")}</Link></Menu.Item>
                    <Menu.Item key="/logout" onClick={logout}>{t("Logout")}</Menu.Item>
                </SubMenu>
            );

            if (myGames && myGames.length > 0) {
                myGamesMenu = (
                    <SubMenu key="mygames" title={t('MyGames')} inlineCollapsed={false}>
                        {myGames.map(g => {
                            const gameTypeInfo = GameTypeInfo.ById(g.gameType);
                            const uri = `/games/${gameTypeInfo.name}/${g.id}`;
                            return (
                                <Menu.Item key={g.id}>
                                    <Link to={uri}>{t(gameTypeInfo.name)}
                                </Link>
                            </Menu.Item>);
                        })}
                    </SubMenu>
                );
            }
        }

        const menuItems = (
            <>
                {myGamesMenu}
                <Menu.Item key="/home"><Link to="/">{t("Home")}</Link></Menu.Item>
                <Menu.Item key="/about"><Link to="/about">{t("About")}</Link></Menu.Item>
                <Menu.Item key="/play"><Link to="/play">{t("Play")}</Link></Menu.Item>
                {languageSelector}
                {accountMenu}
            </>
        );
        setMenuItems(menuItems);

        const setOfDefaultSelectedKeys = new Set();
        const findSelectedKeys = (menuItems, set, level) => {
            let found = false;
            menuItems.forEach(m => {
                let localFound = false;
                if (m.key && pathName.startsWith(m.key)) {
                    localFound = true;
                }
                if (m.props.children) {
                    if (Array.isArray(m.props.children)) {
                        if (findSelectedKeys(m.props.children, set, level + 1)) {
                            localFound = true;
                        }
                    } else {
                        if (m.props.children.key && findSelectedKeys([m.props.children], set, level + 1)) {
                            localFound = true;
                        }
                    }
                }
                if (localFound) {
                    set.add({ key: m.key, level: level });
                    found = true;
                }
            });
            return found;
        };
        findSelectedKeys(menuItems.props.children, setOfDefaultSelectedKeys, 1);
        let defaultSelectedKeys = Array.from(setOfDefaultSelectedKeys).sort((a, b) => a.level < b.level ? -1 : 1).map(m => m.key);
        if (defaultSelectedKeys.length === 0) {
            defaultSelectedKeys = ["/home"]
        }
        setMenuSelected(defaultSelectedKeys);
    }, [pathName, logout, t, user, fetchWithUi, myGames]);

    const onOpenChange = m => {
        setMenuSelected(m);
    };

    return (
        <div>
            <Layout>
                <Sider>
                    <Menu key="menu" theme="dark" mode="inline"
                        defaultOpenKeys={['mygames']}
                        defaultSelectedKeys={['home']}
                        selectedKeys={menuSelected}
                        openKeys={[...menuSelected, 'mygames']}
                        onOpenChange={onOpenChange}
                    >
                        {menuItems.props.children}
                    </Menu>
                </Sider>
                <Layout className="site-layout">
                    <Header>

                    </Header>
                    <Content>
                        {error}
                        <h1><img src={logo} className="App-logo" alt={t("OnlineBoardzLogo")} />{t("OnlineBoardz")}</h1>

                        <Routes
                            user={user}
                            fetchWithUi={fetchWithUi}
                            authenticationStore={authenticationStore}
                            onLoggingError={onLoggingError}
                            gameNotificationClient={gameNotificationClient}
                            gameReachabilityChecker={gameReachabilityChecker}
                            setError={setError}
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
            </Layout>
        </div>
    );
}

export default App;