import { useNavigate } from 'react-router';
import { motion, AnimatePresence } from 'motion/react';
import { useState } from 'react';
import { BlackHoleVisual } from './BlackHoleVisual';
import { useGame, SKINS, Skin } from '../context/GameContext';

const RARITY_COLORS = {
  common: '#9ca3af',
  rare: '#3b82f6',
  epic: '#8b5cf6',
  legendary: '#eab308',
};

const RARITY_LABELS = {
  common: 'Обычный',
  rare: 'Редкий',
  epic: 'Эпический',
  legendary: 'Легендарный',
};

export function ShopScreen() {
  const navigate = useNavigate();
  const game = useGame();
  const [notification, setNotification] = useState<{ msg: string; ok: boolean } | null>(null);
  const [selectedSkin, setSelectedSkin] = useState<Skin | null>(null);

  const handleBuy = (skin: Skin) => {
    if (game.ownedSkins.includes(skin.id)) {
      game.equipSkin(skin.id);
      setNotification({ msg: `«${skin.name}» экипирован!`, ok: true });
    } else {
      const ok = game.buySkin(skin.id, skin.price);
      if (ok) {
        game.equipSkin(skin.id);
        setNotification({ msg: `«${skin.name}» куплен и экипирован!`, ok: true });
      } else {
        setNotification({ msg: 'Недостаточно монет!', ok: false });
      }
    }
    setTimeout(() => setNotification(null), 2000);
  };

  return (
    <div className="min-h-screen flex flex-col px-4 pt-6 pb-10 max-w-2xl mx-auto">

      {/* Header */}
      <motion.div
        className="flex items-center justify-between mb-6"
        initial={{ opacity: 0, y: -15 }}
        animate={{ opacity: 1, y: 0 }}
      >
        <button
          className="flex items-center gap-2 px-4 py-2.5 rounded-xl text-sm font-bold tracking-wider"
          style={{
            background: 'rgba(255,255,255,0.06)',
            border: '1px solid rgba(255,255,255,0.12)',
            color: 'rgba(255,255,255,0.8)',
            fontFamily: "'Orbitron', monospace",
            fontSize: '0.7rem',
          }}
          onClick={() => navigate('/')}
        >
          ← НАЗАД
        </button>

        <h2 style={{ fontFamily: "'Orbitron', monospace", fontWeight: 800, color: 'white', fontSize: '1.3rem', letterSpacing: '0.1em' }}>
          МАГАЗИН
        </h2>

        <div
          className="flex items-center gap-2 px-4 py-2.5 rounded-xl"
          style={{ background: 'rgba(234,179,8,0.12)', border: '1px solid rgba(234,179,8,0.3)' }}
        >
          <span className="text-base">🪙</span>
          <span style={{ color: '#eab308', fontFamily: "'Orbitron', monospace", fontWeight: 700, fontSize: '0.85rem' }}>
            {game.coins.toLocaleString()}
          </span>
        </div>
      </motion.div>

      {/* Preview selected */}
      <AnimatePresence>
        {selectedSkin && (
          <motion.div
            className="mb-5 rounded-2xl p-4 flex items-center gap-5"
            style={{
              background: `linear-gradient(135deg, ${selectedSkin.glowColor}15, rgba(0,0,0,0.4))`,
              border: `1px solid ${selectedSkin.glowColor}40`,
              backdropFilter: 'blur(16px)',
            }}
            initial={{ opacity: 0, y: -10, height: 0 }}
            animate={{ opacity: 1, y: 0, height: 'auto' }}
            exit={{ opacity: 0, y: -10, height: 0 }}
          >
            <BlackHoleVisual glowColor={selectedSkin.glowColor} size={48} intensity={75} />
            <div className="flex-1">
              <div style={{ fontFamily: "'Orbitron', monospace", fontWeight: 700, color: 'white', fontSize: '1rem' }}>
                {selectedSkin.name}
              </div>
              <div style={{ color: 'rgba(255,255,255,0.5)', fontSize: '0.85rem', marginTop: 2 }}>
                {selectedSkin.description}
              </div>
              <div
                className="inline-block px-2 py-0.5 rounded-md mt-1 text-xs font-bold"
                style={{
                  background: `${RARITY_COLORS[selectedSkin.rarity]}20`,
                  color: RARITY_COLORS[selectedSkin.rarity],
                  border: `1px solid ${RARITY_COLORS[selectedSkin.rarity]}50`,
                  fontFamily: "'Orbitron', monospace",
                  fontSize: '0.6rem',
                }}
              >
                {RARITY_LABELS[selectedSkin.rarity].toUpperCase()}
              </div>
            </div>
            <button
              className="px-5 py-3 rounded-xl font-bold text-sm"
              style={{
                fontFamily: "'Orbitron', monospace",
                fontSize: '0.7rem',
                background: game.ownedSkins.includes(selectedSkin.id)
                  ? game.equippedSkin === selectedSkin.id
                    ? `${selectedSkin.glowColor}30`
                    : `${selectedSkin.glowColor}50`
                  : `${selectedSkin.glowColor}cc`,
                border: `1px solid ${selectedSkin.glowColor}`,
                color: 'white',
                boxShadow: game.ownedSkins.includes(selectedSkin.id) ? 'none' : `0 0 16px ${selectedSkin.glowColor}60`,
                opacity: game.equippedSkin === selectedSkin.id ? 0.6 : 1,
              }}
              disabled={game.equippedSkin === selectedSkin.id}
              onClick={() => handleBuy(selectedSkin)}
            >
              {game.equippedSkin === selectedSkin.id
                ? '✓ ЭКИПИРОВАН'
                : game.ownedSkins.includes(selectedSkin.id)
                ? 'ЭКИПИРОВАТЬ'
                : `🪙 ${selectedSkin.price}`}
            </button>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Skins grid */}
      <div className="grid grid-cols-3 gap-3">
        {SKINS.map((skin, i) => {
          const owned = game.ownedSkins.includes(skin.id);
          const equipped = game.equippedSkin === skin.id;
          const selected = selectedSkin?.id === skin.id;
          const rarityColor = RARITY_COLORS[skin.rarity];

          return (
            <motion.button
              key={skin.id}
              className="relative flex flex-col items-center gap-2 py-4 px-2 rounded-2xl"
              style={{
                background: selected
                  ? `linear-gradient(135deg, ${skin.glowColor}25, rgba(0,0,0,0.5))`
                  : equipped
                  ? `linear-gradient(135deg, ${skin.glowColor}15, rgba(0,0,0,0.5))`
                  : 'rgba(255,255,255,0.04)',
                border: equipped
                  ? `2px solid ${skin.glowColor}80`
                  : selected
                  ? `1.5px solid ${skin.glowColor}60`
                  : '1px solid rgba(255,255,255,0.08)',
                boxShadow: equipped ? `0 0 20px ${skin.glowColor}30` : 'none',
              }}
              initial={{ opacity: 0, scale: 0.9 }}
              animate={{ opacity: 1, scale: 1 }}
              transition={{ delay: i * 0.05 }}
              whileHover={{ scale: 1.04 }}
              whileTap={{ scale: 0.97 }}
              onClick={() => setSelectedSkin(selected ? null : skin)}
            >
              {/* Rarity badge */}
              <div
                className="absolute top-2 left-2 px-1.5 py-0.5 rounded text-xs"
                style={{
                  background: `${rarityColor}20`,
                  border: `1px solid ${rarityColor}50`,
                  color: rarityColor,
                  fontFamily: "'Orbitron', monospace",
                  fontSize: '0.5rem',
                  fontWeight: 700,
                }}
              >
                {skin.rarity.toUpperCase()}
              </div>

              {/* Equipped check */}
              {equipped && (
                <div
                  className="absolute top-2 right-2 w-5 h-5 rounded-full flex items-center justify-center text-xs"
                  style={{ background: skin.glowColor, color: 'white' }}
                >
                  ✓
                </div>
              )}

              <BlackHoleVisual glowColor={skin.glowColor} size={38} intensity={65} />

              <div style={{ fontFamily: "'Orbitron', monospace", fontWeight: 700, color: 'white', fontSize: '0.65rem', textAlign: 'center' }}>
                {skin.name.toUpperCase()}
              </div>

              {!owned ? (
                <div
                  className="flex items-center gap-1 px-2 py-1 rounded-lg text-xs"
                  style={{ background: 'rgba(234,179,8,0.15)', color: '#eab308', fontFamily: "'Orbitron', monospace", fontSize: '0.6rem', fontWeight: 700 }}
                >
                  🪙 {skin.price}
                </div>
              ) : (
                <div
                  className="text-xs px-2 py-1 rounded-lg"
                  style={{
                    background: equipped ? `${skin.glowColor}30` : 'rgba(255,255,255,0.08)',
                    color: equipped ? skin.glowColor : 'rgba(255,255,255,0.5)',
                    fontFamily: "'Orbitron', monospace",
                    fontSize: '0.55rem',
                    fontWeight: 700,
                  }}
                >
                  {equipped ? '✓ ЭКИПИРОВАН' : 'ПОЛУЧЕН'}
                </div>
              )}
            </motion.button>
          );
        })}
      </div>

      {/* Notification */}
      <AnimatePresence>
        {notification && (
          <motion.div
            className="fixed bottom-8 left-1/2 -translate-x-1/2 px-6 py-3 rounded-2xl text-sm font-bold"
            style={{
              background: notification.ok ? 'rgba(34,197,94,0.2)' : 'rgba(239,68,68,0.2)',
              border: `1px solid ${notification.ok ? 'rgba(34,197,94,0.5)' : 'rgba(239,68,68,0.5)'}`,
              color: notification.ok ? '#22c55e' : '#ef4444',
              fontFamily: "'Orbitron', monospace",
              fontSize: '0.75rem',
              backdropFilter: 'blur(12px)',
              zIndex: 100,
              whiteSpace: 'nowrap',
            }}
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: 20 }}
          >
            {notification.ok ? '✓' : '✗'} {notification.msg}
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}
