import { createBrowserRouter } from 'react-router';
import { Layout } from './components/Layout';
import { MainMenu } from './components/MainMenu';
import { ShopScreen } from './components/ShopScreen';
import { MapSelect } from './components/MapSelect';
import { CustomizeScreen } from './components/CustomizeScreen';
import { SettingsScreen } from './components/SettingsScreen';

export const router = createBrowserRouter([
  {
    path: '/',
    Component: Layout,
    children: [
      { index: true, Component: MainMenu },
      { path: 'shop', Component: ShopScreen },
      { path: 'maps', Component: MapSelect },
      { path: 'customize', Component: CustomizeScreen },
      { path: 'settings', Component: SettingsScreen },
    ],
  },
]);
