import { useNavigate } from 'react-router';
import { motion } from 'motion/react';
import { BlackHoleVisual } from './BlackHoleVisual';
import { useGame, MAPS } from '../context/GameContext';

const navItems = [
  { path: '/shop', label: 'Магазин', icon: '🛒', desc: 'Скины' },
  { path: '/maps', label: 'Карты', icon: '🗺️', desc: 'Выбор' },
  { path: '/customize', label: 'Кастом', icon: '✨', desc: 'Подсветка' },
  { path: '/settings', label: 'Управление', icon: '🎮', desc: 'Настройки' },
];

export function MainMenu() {
  const navigate = useNavigate();
  const game = useGame();
  const selectedMap = MAPS.find(m => m.id === game.selectedMap);

  return (
    <div className="min-h-screen flex flex-col items-center justify-between px-4 pt-6 pb-8 relative overflow-hidden">

      {/* Top bar */}
      <motion.div
        className="w-full max-w-lg flex items-center justify-between"
        initial={{ opacity: 0, y: -20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5 }}
      >
        {/* Player info */}
        <div
          className="flex items-center gap-3 px-4 py-2.5 rounded-2xl"
          style={{
            background: 'rgba(255,255,255,0.05)',
            border: '1px solid rgba(255,255,255,0.1)',
            backdropFilter: 'blur(12px)',
          }}
        >
          <div
            className="w-10 h-10 rounded-full flex items-center justify-center text-lg font-bold"
            style={{
              background: `radial-gradient(circle, ${game.glowColor}40, ${game.glowColor}15)`,
              border: `2px solid ${game.glowColor}80`,
              boxShadow: `0 0 12px ${game.glowColor}60`,
              fontFamily: "'Orbitron', monospace",
              color: game.glowColor,
            }}
          >
            {game.playerName[0]?.toUpperCase() || 'P'}
          </div>
          <div>
            <div className="text-white text-sm" style={{ fontFamily: "'Orbitron', monospace", fontWeight: 600 }}>
              {game.playerName}
            </div>
            <div className="text-xs" style={{ color: 'rgba(255,255,255,0.5)' }}>
              LVL {game.level}
            </div>
          </div>
        </div>

        {/* Coins */}
        <div
          className="flex items-center gap-2 px-4 py-2.5 rounded-2xl"
          style={{
            background: 'rgba(234,179,8,0.1)',
            border: '1px solid rgba(234,179,8,0.3)',
            backdropFilter: 'blur(12px)',
          }}
        >
          <span className="text-lg">🪙</span>
          <span
            className="font-bold text-sm"
            style={{ color: '#eab308', fontFamily: "'Orbitron', monospace" }}
          >
            {game.coins.toLocaleString()}
          </span>
        </div>
      </motion.div>

      {/* Center section */}
      <div className="flex flex-col items-center gap-6">
        {/* Title */}
        <motion.div
          className="text-center"
          initial={{ opacity: 0, scale: 0.8 }}
          animate={{ opacity: 1, scale: 1 }}
          transition={{ duration: 0.7, delay: 0.1 }}
        >
          <h1
            className="tracking-widest select-none"
            style={{
              fontFamily: "'Orbitron', monospace",
              fontSize: 'clamp(2rem, 8vw, 3.5rem)',
              fontWeight: 900,
              color: 'white',
              textShadow: `0 0 30px ${game.glowColor}cc, 0 0 60px ${game.glowColor}80, 0 0 90px ${game.glowColor}40`,
              letterSpacing: '0.08em',
            }}
          >
            BLACK HOLE
          </h1>
          <div
            style={{
              fontFamily: "'Orbitron', monospace",
              fontSize: 'clamp(1.4rem, 5vw, 2.5rem)',
              fontWeight: 700,
              color: game.glowColor,
              textShadow: `0 0 20px ${game.glowColor}`,
              letterSpacing: '0.2em',
              marginTop: '-0.2em',
            }}
          >
            .IO
          </div>
        </motion.div>

        {/* Black hole */}
        <motion.div
          initial={{ opacity: 0, scale: 0.6 }}
          animate={{ opacity: 1, scale: 1 }}
          transition={{ duration: 0.8, delay: 0.2, type: 'spring', damping: 12 }}
        >
          <BlackHoleVisual
            glowColor={game.glowColor}
            size={100}
            intensity={game.glowIntensity}
          />
        </motion.div>

        {/* Stats row */}
        <motion.div
          className="flex items-center gap-4"
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ delay: 0.4 }}
        >
          <StatBadge icon="🏆" label="Рекорд" value={game.highScore.toLocaleString()} color="#eab308" />
          <StatBadge icon="🗺️" label="Карта" value={selectedMap?.name || '—'} color={game.glowColor} />
        </motion.div>

        {/* Play button */}
        <motion.button
          className="relative overflow-hidden rounded-2xl px-16 py-5 font-black tracking-widest uppercase select-none"
          style={{
            fontFamily: "'Orbitron', monospace",
            fontSize: '1.25rem',
            color: '#ffffff',
            background: `linear-gradient(135deg, ${game.glowColor}cc, ${game.glowColor}88)`,
            border: `2px solid ${game.glowColor}`,
            boxShadow: `0 0 30px ${game.glowColor}80, 0 0 60px ${game.glowColor}40, inset 0 1px 0 rgba(255,255,255,0.2)`,
          }}
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.5, duration: 0.5 }}
          whileHover={{
            scale: 1.06,
            boxShadow: `0 0 50px ${game.glowColor}aa, 0 0 80px ${game.glowColor}60`,
          }}
          whileTap={{ scale: 0.97 }}
        >
          {/* Shimmer */}
          <motion.div
            className="absolute inset-0 pointer-events-none"
            style={{
              background: 'linear-gradient(90deg, transparent 0%, rgba(255,255,255,0.15) 50%, transparent 100%)',
              transform: 'skewX(-20deg)',
            }}
            animate={{ x: ['-200%', '200%'] }}
            transition={{ duration: 2.5, repeat: Infinity, ease: 'easeInOut', repeatDelay: 1 }}
          />
          ▶ &nbsp;ИГРАТЬ
        </motion.button>
      </div>

      {/* Bottom nav */}
      <motion.div
        className="w-full max-w-lg grid grid-cols-4 gap-3"
        initial={{ opacity: 0, y: 30 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.6, duration: 0.5 }}
      >
        {navItems.map((item, i) => (
          <motion.button
            key={item.path}
            className="flex flex-col items-center gap-2 py-4 rounded-2xl"
            style={{
              background: 'rgba(255,255,255,0.05)',
              border: '1px solid rgba(255,255,255,0.1)',
              backdropFilter: 'blur(12px)',
              color: 'rgba(255,255,255,0.9)',
            }}
            whileHover={{
              background: `${game.glowColor}20`,
              borderColor: `${game.glowColor}60`,
              scale: 1.04,
            }}
            whileTap={{ scale: 0.96 }}
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.7 + i * 0.07 }}
            onClick={() => navigate(item.path)}
          >
            <span className="text-2xl">{item.icon}</span>
            <span className="text-xs font-bold tracking-wider uppercase" style={{ fontFamily: "'Orbitron', monospace", fontSize: '0.6rem' }}>
              {item.label}
            </span>
          </motion.button>
        ))}
      </motion.div>
    </div>
  );
}

function StatBadge({ icon, label, value, color }: { icon: string; label: string; value: string; color: string }) {
  return (
    <div
      className="flex items-center gap-2 px-4 py-2 rounded-xl"
      style={{
        background: 'rgba(255,255,255,0.05)',
        border: `1px solid ${color}30`,
      }}
    >
      <span className="text-base">{icon}</span>
      <div>
        <div className="text-xs" style={{ color: 'rgba(255,255,255,0.4)', lineHeight: 1 }}>{label}</div>
        <div className="text-sm font-bold" style={{ color, fontFamily: "'Orbitron', monospace", fontSize: '0.75rem' }}>{value}</div>
      </div>
    </div>
  );
}
