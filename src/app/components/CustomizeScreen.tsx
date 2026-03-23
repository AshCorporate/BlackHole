import { useNavigate } from 'react-router';
import { motion } from 'motion/react';
import { useState } from 'react';
import { BlackHoleVisual } from './BlackHoleVisual';
import { useGame, GLOW_COLORS } from '../context/GameContext';

const RING_STYLES = [
  { id: 'orbital', label: 'Орбитальный', icon: '🪐' },
  { id: 'pulse', label: 'Пульсация', icon: '💫' },
  { id: 'electric', label: 'Электрика', icon: '⚡' },
  { id: 'flame', label: 'Огненный', icon: '🔥' },
];

export function CustomizeScreen() {
  const navigate = useNavigate();
  const game = useGame();
  const [previewColor, setPreviewColor] = useState(game.glowColor);
  const [previewIntensity, setPreviewIntensity] = useState(game.glowIntensity);
  const [ringStyle, setRingStyle] = useState('orbital');
  const [saved, setSaved] = useState(false);

  const handleSave = () => {
    game.setGlowColor(previewColor);
    game.setGlowIntensity(previewIntensity);
    setSaved(true);
    setTimeout(() => setSaved(false), 1800);
  };

  return (
    <div className="min-h-screen flex flex-col px-4 pt-6 pb-10 max-w-lg mx-auto">

      {/* Header */}
      <motion.div
        className="flex items-center justify-between mb-6"
        initial={{ opacity: 0, y: -15 }}
        animate={{ opacity: 1, y: 0 }}
      >
        <button
          className="flex items-center gap-2 px-4 py-2.5 rounded-xl text-sm font-bold"
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
          КАСТОМИЗАЦИЯ
        </h2>

        <div style={{ width: 80 }} />
      </motion.div>

      {/* Preview area */}
      <motion.div
        className="flex flex-col items-center py-6 rounded-3xl mb-6"
        style={{
          background: `radial-gradient(ellipse at 50% 50%, ${previewColor}12 0%, rgba(0,0,0,0.5) 70%)`,
          border: `1px solid ${previewColor}30`,
        }}
        initial={{ opacity: 0, scale: 0.9 }}
        animate={{ opacity: 1, scale: 1 }}
        transition={{ delay: 0.15 }}
      >
        <BlackHoleVisual
          glowColor={previewColor}
          size={90}
          intensity={previewIntensity}
        />
        <div
          className="mt-2 text-sm font-bold"
          style={{ fontFamily: "'Orbitron', monospace", color: previewColor, fontSize: '0.7rem', letterSpacing: '0.15em' }}
        >
          ПРЕДПРОСМОТР
        </div>
      </motion.div>

      {/* Color palette */}
      <motion.div
        className="mb-6"
        initial={{ opacity: 0, y: 10 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.2 }}
      >
        <SectionTitle icon="🎨" label="Цвет подсветки" />
        <div className="grid grid-cols-6 gap-3 mt-3">
          {GLOW_COLORS.map((c) => (
            <motion.button
              key={c.color}
              className="relative flex flex-col items-center gap-1.5"
              whileHover={{ scale: 1.15 }}
              whileTap={{ scale: 0.9 }}
              onClick={() => setPreviewColor(c.color)}
              title={c.name}
            >
              <div
                className="w-10 h-10 rounded-full"
                style={{
                  background: `radial-gradient(circle at 35% 30%, ${c.color}dd, ${c.color}66)`,
                  boxShadow: previewColor === c.color
                    ? `0 0 16px ${c.color}cc, 0 0 6px ${c.color}`
                    : `0 0 8px ${c.color}40`,
                  border: previewColor === c.color
                    ? `2.5px solid ${c.color}`
                    : '2px solid rgba(255,255,255,0.1)',
                  transform: previewColor === c.color ? 'scale(1.15)' : 'scale(1)',
                  transition: 'all 0.2s',
                }}
              />
              {previewColor === c.color && (
                <motion.div
                  className="w-1.5 h-1.5 rounded-full"
                  style={{ background: c.color }}
                  initial={{ scale: 0 }}
                  animate={{ scale: 1 }}
                />
              )}
            </motion.button>
          ))}
        </div>

        {/* Custom hex color */}
        <div className="mt-4 flex items-center gap-3">
          <label style={{ color: 'rgba(255,255,255,0.5)', fontSize: '0.8rem', fontFamily: "'Orbitron', monospace', minWidth: 100" }}>
            Свой цвет:
          </label>
          <div className="flex items-center gap-2 flex-1">
            <input
              type="color"
              value={previewColor}
              onChange={e => setPreviewColor(e.target.value)}
              className="w-10 h-10 rounded-xl cursor-pointer"
              style={{ background: 'none', border: `2px solid ${previewColor}60` }}
            />
            <input
              type="text"
              value={previewColor}
              onChange={e => {
                if (/^#[0-9a-fA-F]{0,6}$/.test(e.target.value)) setPreviewColor(e.target.value);
              }}
              className="flex-1 px-3 py-2 rounded-xl text-sm font-mono"
              style={{
                background: 'rgba(255,255,255,0.06)',
                border: `1px solid ${previewColor}40`,
                color: previewColor,
                fontSize: '0.85rem',
              }}
            />
          </div>
        </div>
      </motion.div>

      {/* Intensity slider */}
      <motion.div
        className="mb-6"
        initial={{ opacity: 0, y: 10 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.25 }}
      >
        <SectionTitle icon="💡" label={`Интенсивность свечения: ${previewIntensity}%`} />
        <div className="mt-3 px-1">
          <input
            type="range"
            min={20}
            max={100}
            value={previewIntensity}
            onChange={e => setPreviewIntensity(Number(e.target.value))}
            className="w-full h-2 rounded-full appearance-none cursor-pointer"
            style={{
              background: `linear-gradient(to right, ${previewColor} 0%, ${previewColor} ${previewIntensity}%, rgba(255,255,255,0.12) ${previewIntensity}%, rgba(255,255,255,0.12) 100%)`,
              accentColor: previewColor,
            }}
          />
          <div className="flex justify-between mt-1">
            <span style={{ color: 'rgba(255,255,255,0.3)', fontSize: '0.7rem' }}>Слабое</span>
            <span style={{ color: 'rgba(255,255,255,0.3)', fontSize: '0.7rem' }}>Сильное</span>
          </div>
        </div>
      </motion.div>

      {/* Ring style */}
      <motion.div
        className="mb-8"
        initial={{ opacity: 0, y: 10 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.3 }}
      >
        <SectionTitle icon="🌀" label="Стиль колец" />
        <div className="grid grid-cols-4 gap-2 mt-3">
          {RING_STYLES.map(style => (
            <motion.button
              key={style.id}
              className="flex flex-col items-center gap-1.5 py-3 px-2 rounded-xl"
              style={{
                background: ringStyle === style.id
                  ? `${previewColor}25`
                  : 'rgba(255,255,255,0.04)',
                border: ringStyle === style.id
                  ? `1.5px solid ${previewColor}70`
                  : '1px solid rgba(255,255,255,0.08)',
                boxShadow: ringStyle === style.id ? `0 0 12px ${previewColor}20` : 'none',
              }}
              whileHover={{ scale: 1.05 }}
              whileTap={{ scale: 0.96 }}
              onClick={() => setRingStyle(style.id)}
            >
              <span className="text-xl">{style.icon}</span>
              <span style={{ color: ringStyle === style.id ? previewColor : 'rgba(255,255,255,0.5)', fontFamily: "'Orbitron', monospace", fontSize: '0.55rem', fontWeight: 700 }}>
                {style.label.toUpperCase()}
              </span>
            </motion.button>
          ))}
        </div>
      </motion.div>

      {/* Save button */}
      <motion.button
        className="w-full py-4 rounded-2xl font-black tracking-widest uppercase relative overflow-hidden"
        style={{
          fontFamily: "'Orbitron', monospace",
          fontSize: '0.9rem',
          color: 'white',
          background: saved
            ? 'linear-gradient(135deg, #22c55ecc, #22c55e88)'
            : `linear-gradient(135deg, ${previewColor}cc, ${previewColor}88)`,
          border: `1.5px solid ${saved ? '#22c55e' : previewColor}`,
          boxShadow: `0 0 24px ${saved ? '#22c55e60' : `${previewColor}60`}`,
          transition: 'all 0.4s',
        }}
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.4 }}
        whileHover={{ scale: 1.03 }}
        whileTap={{ scale: 0.97 }}
        onClick={handleSave}
      >
        {saved ? '✓ СОХРАНЕНО!' : '💾 СОХРАНИТЬ НАСТРОЙКИ'}
      </motion.button>
    </div>
  );
}

function SectionTitle({ icon, label }: { icon: string; label: string }) {
  return (
    <div className="flex items-center gap-2">
      <span className="text-base">{icon}</span>
      <span style={{ fontFamily: "'Orbitron', monospace", fontWeight: 600, color: 'rgba(255,255,255,0.75)', fontSize: '0.75rem', letterSpacing: '0.08em' }}>
        {label.toUpperCase()}
      </span>
    </div>
  );
}
