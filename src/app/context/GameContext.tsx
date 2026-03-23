import React, { createContext, useContext, useState, useEffect } from 'react';

export interface Skin {
  id: string;
  name: string;
  description: string;
  price: number;
  glowColor: string;
  coreColor: string;
  rarity: 'common' | 'rare' | 'epic' | 'legendary';
}

export interface GameMap {
  id: string;
  name: string;
  description: string;
  difficulty: 'Easy' | 'Medium' | 'Hard';
  emoji: string;
  bgFrom: string;
  bgTo: string;
}

export const SKINS: Skin[] = [
  { id: 'void', name: 'Void', description: 'Классическая тёмная пустота', price: 0, glowColor: '#8b5cf6', coreColor: '#000', rarity: 'common' },
  { id: 'nebula', name: 'Nebula', description: 'Вихри космической туманности', price: 500, glowColor: '#c026d3', coreColor: '#1a0a2e', rarity: 'rare' },
  { id: 'solar', name: 'Solar Flare', description: 'Энергия солнечной вспышки', price: 750, glowColor: '#f97316', coreColor: '#0a0500', rarity: 'rare' },
  { id: 'arctic', name: 'Arctic', description: 'Замороженная энергия пустоты', price: 750, glowColor: '#06b6d4', coreColor: '#000a10', rarity: 'rare' },
  { id: 'toxic', name: 'Toxic', description: 'Радиоактивная сингулярность', price: 1000, glowColor: '#22c55e', coreColor: '#000a00', rarity: 'epic' },
  { id: 'inferno', name: 'Inferno', description: 'Адское пламя сингулярности', price: 1000, glowColor: '#ef4444', coreColor: '#0a0000', rarity: 'epic' },
  { id: 'phantom', name: 'Phantom', description: 'Эфирный призрачный вихрь', price: 1500, glowColor: '#e2e8f0', coreColor: '#05050a', rarity: 'epic' },
  { id: 'royal', name: 'Royal', description: 'Величественная космическая мощь', price: 2000, glowColor: '#eab308', coreColor: '#0a0500', rarity: 'legendary' },
  { id: 'abyss', name: 'Abyss', description: 'Бездна за пределами вселенной', price: 3000, glowColor: '#6366f1', coreColor: '#000005', rarity: 'legendary' },
];

export const MAPS: GameMap[] = [
  { id: 'city', name: 'Мегаполис', description: 'Городской пейзаж с зданиями и машинами', difficulty: 'Easy', emoji: '🏙️', bgFrom: '#0f172a', bgTo: '#1e293b' },
  { id: 'space', name: 'Орбитальная Станция', description: 'Невесомость в открытом космосе', difficulty: 'Medium', emoji: '🚀', bgFrom: '#020617', bgTo: '#0f0a2e' },
  { id: 'desert', name: 'Сахара', description: 'Бескрайняя пустыня с оазисами', difficulty: 'Easy', emoji: '🏜️', bgFrom: '#1c0a00', bgTo: '#291400' },
  { id: 'ocean', name: 'Глубокий Океан', description: 'Подводное царство с существами', difficulty: 'Hard', emoji: '🌊', bgFrom: '#001a2e', bgTo: '#002a47' },
  { id: 'arctic', name: 'Ледяная Тундра', description: 'Льды и снежный пейзаж', difficulty: 'Medium', emoji: '❄️', bgFrom: '#0a1628', bgTo: '#0d1f35' },
  { id: 'volcano', name: 'Вулканический Остров', description: 'Лавовые потоки и вулканы', difficulty: 'Hard', emoji: '🌋', bgFrom: '#1a0000', bgTo: '#2d0500' },
];

export const GLOW_COLORS = [
  { color: '#8b5cf6', name: 'Фиолетовый' },
  { color: '#c026d3', name: 'Маджента' },
  { color: '#06b6d4', name: 'Циан' },
  { color: '#22c55e', name: 'Зелёный' },
  { color: '#ef4444', name: 'Красный' },
  { color: '#f97316', name: 'Оранжевый' },
  { color: '#eab308', name: 'Золотой' },
  { color: '#e2e8f0', name: 'Белый' },
  { color: '#6366f1', name: 'Индиго' },
  { color: '#ec4899', name: 'Розовый' },
  { color: '#14b8a6', name: 'Бирюзовый' },
  { color: '#f43f5e', name: 'Алый' },
];

interface GameState {
  coins: number;
  equippedSkin: string;
  ownedSkins: string[];
  selectedMap: string;
  glowColor: string;
  glowIntensity: number;
  controlMode: string;
  playerName: string;
  level: number;
  highScore: number;
}

interface GameContextType extends GameState {
  buySkin: (skinId: string, price: number) => boolean;
  equipSkin: (skinId: string) => void;
  selectMap: (mapId: string) => void;
  setGlowColor: (color: string) => void;
  setGlowIntensity: (intensity: number) => void;
  setControlMode: (mode: string) => void;
  setPlayerName: (name: string) => void;
  addCoins: (amount: number) => void;
}

const defaultState: GameState = {
  coins: 1500,
  equippedSkin: 'void',
  ownedSkins: ['void'],
  selectedMap: 'city',
  glowColor: '#8b5cf6',
  glowIntensity: 70,
  controlMode: 'wasd',
  playerName: 'Player',
  level: 7,
  highScore: 18420,
};

const GameContext = createContext<GameContextType | null>(null);

export function GameProvider({ children }: { children: React.ReactNode }) {
  const [state, setState] = useState<GameState>(() => {
    try {
      const saved = localStorage.getItem('blackhole-game-state');
      return saved ? { ...defaultState, ...JSON.parse(saved) } : defaultState;
    } catch {
      return defaultState;
    }
  });

  useEffect(() => {
    localStorage.setItem('blackhole-game-state', JSON.stringify(state));
  }, [state]);

  const buySkin = (skinId: string, price: number): boolean => {
    if (state.coins < price || state.ownedSkins.includes(skinId)) return false;
    setState(s => ({ ...s, coins: s.coins - price, ownedSkins: [...s.ownedSkins, skinId] }));
    return true;
  };

  const equipSkin = (skinId: string) => {
    if (!state.ownedSkins.includes(skinId)) return;
    const skin = SKINS.find(s => s.id === skinId);
    setState(s => ({ ...s, equippedSkin: skinId, glowColor: skin?.glowColor || s.glowColor }));
  };

  const selectMap = (mapId: string) => setState(s => ({ ...s, selectedMap: mapId }));
  const setGlowColor = (color: string) => setState(s => ({ ...s, glowColor: color }));
  const setGlowIntensity = (intensity: number) => setState(s => ({ ...s, glowIntensity: intensity }));
  const setControlMode = (mode: string) => setState(s => ({ ...s, controlMode: mode }));
  const setPlayerName = (name: string) => setState(s => ({ ...s, playerName: name }));
  const addCoins = (amount: number) => setState(s => ({ ...s, coins: s.coins + amount }));

  return (
    <GameContext.Provider value={{ ...state, buySkin, equipSkin, selectMap, setGlowColor, setGlowIntensity, setControlMode, setPlayerName, addCoins }}>
      {children}
    </GameContext.Provider>
  );
}

export function useGame() {
  const ctx = useContext(GameContext);
  if (!ctx) throw new Error('useGame must be used within GameProvider');
  return ctx;
}
