import { useNavigate } from 'react-router';
import { motion } from 'motion/react';
import { useState } from 'react';
import { useGame } from '../context/GameContext';

const CONTROLS = [
  {
    id: 'wasd',
    name: 'WASD / Стрелки',
    description: 'Классическое управление с клавиатуры',
    icon: '⌨️',
    keys: ['W', 'A', 'S', 'D'],
    color: '#8b5cf6',
  },
  {
    id: 'mouse',
    name: 'Мышь',
    description: 'Чёрная дыра следует за курсором',
    icon: '🖱️',
    keys: [],
    color: '#06b6d4',
  },
  {
    id: 'gamepad',
    name: 'Геймпад',
    description: 'Поддержка контроллера Xbox / PS',
    icon: '🎮',
    keys: [],
    color: '#22c55e',
  },
  {
    id: 'touch',
    name: 'Сенсор',
    description: 'Управление касанием для мобильных',
    icon: '📱',
    keys: [],
    color: '#f97316',
  },
];

const QUALITY_OPTIONS = ['Низкое', 'Среднее', 'Высокое', 'Ультра'];

export function SettingsScreen() {
  const navigate = useNavigate();
  const game = useGame();
  const [soundVol, setSoundVol] = useState(70);
  const [musicVol, setMusicVol] = useState(50);
  const [quality, setQuality] = useState(2);
  const [particles, setParticles] = useState(true);
  const [screenShake, setScreenShake] = useState(true);
  const [saved, setSaved] = useState(false);

  const handleSave = () => {
    game.setControlMode(game.controlMode);
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
          УПРАВЛЕНИЕ
        </h2>

        <div style={{ width: 80 }} />
      </motion.div>

      {/* Control mode section */}
      <motion.div
        className="mb-6"
        initial={{ opacity: 0, y: 10 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.1 }}
      >
        <SectionTitle icon="🕹️" label="Режим управления" />
        <div className="grid grid-cols-2 gap-3 mt-3">
          {CONTROLS.map((ctrl, i) => {
            const active = game.controlMode === ctrl.id;
            return (
              <motion.button
                key={ctrl.id}
                className="relative flex flex-col gap-3 p-4 rounded-2xl text-left overflow-hidden"
                style={{
                  background: active
                    ? `linear-gradient(135deg, ${ctrl.color}20, rgba(0,0,0,0.5))`
                    : 'rgba(255,255,255,0.04)',
                  border: active
                    ? `2px solid ${ctrl.color}70`
                    : '1px solid rgba(255,255,255,0.08)',
                  boxShadow: active ? `0 0 20px ${ctrl.color}25` : 'none',
                }}
                initial={{ opacity: 0, scale: 0.9 }}
                animate={{ opacity: 1, scale: 1 }}
                transition={{ delay: 0.1 + i * 0.07 }}
                whileHover={{ scale: 1.03 }}
                whileTap={{ scale: 0.97 }}
                onClick={() => game.setControlMode(ctrl.id)}
              >
                {active && (
                  <motion.div
                    className="absolute top-3 right-3 w-6 h-6 rounded-full flex items-center justify-center text-xs font-bold"
                    style={{ background: ctrl.color, color: 'white' }}
                    initial={{ scale: 0 }}
                    animate={{ scale: 1 }}
                    transition={{ type: 'spring' }}
                  >
                    ✓
                  </motion.div>
                )}

                <div className="flex items-center gap-2">
                  <span className="text-2xl">{ctrl.icon}</span>
                  {ctrl.keys.length > 0 && (
                    <div className="flex gap-1">
                      {ctrl.keys.map(k => (
                        <div
                          key={k}
                          className="px-1.5 py-0.5 rounded text-xs font-bold"
                          style={{
                            background: 'rgba(255,255,255,0.1)',
                            border: '1px solid rgba(255,255,255,0.2)',
                            color: 'rgba(255,255,255,0.8)',
                            fontFamily: 'monospace',
                            fontSize: '0.65rem',
                          }}
                        >
                          {k}
                        </div>
                      ))}
                    </div>
                  )}
                </div>

                <div>
                  <div style={{ fontFamily: "'Orbitron', monospace", fontWeight: 700, color: active ? ctrl.color : 'white', fontSize: '0.7rem' }}>
                    {ctrl.name.toUpperCase()}
                  </div>
                  <div style={{ color: 'rgba(255,255,255,0.45)', fontSize: '0.72rem', marginTop: 2, lineHeight: 1.3 }}>
                    {ctrl.description}
                  </div>
                </div>

                {/* Active glow line */}
                {active && (
                  <div
                    className="absolute bottom-0 left-0 right-0 h-0.5"
                    style={{ background: `linear-gradient(to right, transparent, ${ctrl.color}, transparent)` }}
                  />
                )}
              </motion.button>
            );
          })}
        </div>
      </motion.div>

      {/* Audio section */}
      <motion.div
        className="mb-6"
        initial={{ opacity: 0, y: 10 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.25 }}
      >
        <SectionTitle icon="🔊" label="Звук" />
        <div
          className="mt-3 p-4 rounded-2xl flex flex-col gap-4"
          style={{ background: 'rgba(255,255,255,0.04)', border: '1px solid rgba(255,255,255,0.08)' }}
        >
          <VolumeSlider
            label="Звуковые эффекты"
            value={soundVol}
            onChange={setSoundVol}
            color={game.glowColor}
            icon="🎵"
          />
          <VolumeSlider
            label="Музыка"
            value={musicVol}
            onChange={setMusicVol}
            color={game.glowColor}
            icon="🎶"
          />
        </div>
      </motion.div>

      {/* Graphics section */}
      <motion.div
        className="mb-8"
        initial={{ opacity: 0, y: 10 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.3 }}
      >
        <SectionTitle icon="⚙️" label="Графика" />
        <div
          className="mt-3 p-4 rounded-2xl flex flex-col gap-4"
          style={{ background: 'rgba(255,255,255,0.04)', border: '1px solid rgba(255,255,255,0.08)' }}
        >
          {/* Quality */}
          <div>
            <div className="flex justify-between items-center mb-2">
              <span style={{ color: 'rgba(255,255,255,0.6)', fontSize: '0.8rem', fontFamily: "'Orbitron', monospace", fontSize: '0.7rem' }}>
                КАЧЕСТВО
              </span>
              <span style={{ color: game.glowColor, fontFamily: "'Orbitron', monospace", fontSize: '0.7rem', fontWeight: 700 }}>
                {QUALITY_OPTIONS[quality]}
              </span>
            </div>
            <div className="grid grid-cols-4 gap-1.5">
              {QUALITY_OPTIONS.map((q, i) => (
                <button
                  key={q}
                  className="py-2 rounded-lg text-xs font-bold"
                  style={{
                    background: quality === i ? `${game.glowColor}30` : 'rgba(255,255,255,0.05)',
                    border: quality === i ? `1px solid ${game.glowColor}70` : '1px solid rgba(255,255,255,0.1)',
                    color: quality === i ? game.glowColor : 'rgba(255,255,255,0.4)',
                    fontFamily: "'Orbitron', monospace",
                    fontSize: '0.55rem',
                  }}
                  onClick={() => setQuality(i)}
                >
                  {q.toUpperCase()}
                </button>
              ))}
            </div>
          </div>

          {/* Toggles */}
          <ToggleRow
            label="Частицы"
            icon="✨"
            value={particles}
            onChange={setParticles}
            color={game.glowColor}
          />
          <ToggleRow
            label="Тряска экрана"
            icon="📳"
            value={screenShake}
            onChange={setScreenShake}
            color={game.glowColor}
          />
        </div>
      </motion.div>

      {/* Save */}
      <motion.button
        className="w-full py-4 rounded-2xl font-black tracking-widest uppercase"
        style={{
          fontFamily: "'Orbitron', monospace",
          fontSize: '0.9rem',
          color: 'white',
          background: saved
            ? 'linear-gradient(135deg, #22c55ecc, #22c55e88)'
            : `linear-gradient(135deg, ${game.glowColor}cc, ${game.glowColor}88)`,
          border: `1.5px solid ${saved ? '#22c55e' : game.glowColor}`,
          boxShadow: `0 0 24px ${saved ? '#22c55e60' : `${game.glowColor}60`}`,
          transition: 'all 0.4s',
        }}
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.45 }}
        whileHover={{ scale: 1.03 }}
        whileTap={{ scale: 0.97 }}
        onClick={handleSave}
      >
        {saved ? '✓ СОХРАНЕНО!' : '💾 СОХРАНИТЬ'}
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

function VolumeSlider({ label, value, onChange, color, icon }: {
  label: string; value: number; onChange: (v: number) => void; color: string; icon: string;
}) {
  return (
    <div>
      <div className="flex items-center justify-between mb-1.5">
        <div className="flex items-center gap-1.5">
          <span className="text-sm">{icon}</span>
          <span style={{ color: 'rgba(255,255,255,0.6)', fontFamily: "'Orbitron', monospace", fontSize: '0.65rem' }}>
            {label.toUpperCase()}
          </span>
        </div>
        <span style={{ color, fontFamily: "'Orbitron', monospace", fontSize: '0.7rem', fontWeight: 700 }}>
          {value}%
        </span>
      </div>
      <input
        type="range"
        min={0}
        max={100}
        value={value}
        onChange={e => onChange(Number(e.target.value))}
        className="w-full h-1.5 rounded-full appearance-none cursor-pointer"
        style={{
          background: `linear-gradient(to right, ${color} 0%, ${color} ${value}%, rgba(255,255,255,0.1) ${value}%, rgba(255,255,255,0.1) 100%)`,
          accentColor: color,
        }}
      />
    </div>
  );
}

function ToggleRow({ label, icon, value, onChange, color }: {
  label: string; icon: string; value: boolean; onChange: (v: boolean) => void; color: string;
}) {
  return (
    <div className="flex items-center justify-between">
      <div className="flex items-center gap-2">
        <span className="text-base">{icon}</span>
        <span style={{ color: 'rgba(255,255,255,0.7)', fontFamily: "'Orbitron', monospace", fontSize: '0.7rem' }}>
          {label.toUpperCase()}
        </span>
      </div>
      <button
        className="relative w-12 h-6 rounded-full transition-all duration-300"
        style={{
          background: value ? `${color}80` : 'rgba(255,255,255,0.1)',
          border: `1px solid ${value ? color : 'rgba(255,255,255,0.15)'}`,
          boxShadow: value ? `0 0 10px ${color}50` : 'none',
        }}
        onClick={() => onChange(!value)}
      >
        <motion.div
          className="absolute top-0.5 w-5 h-5 rounded-full"
          style={{ background: value ? color : 'rgba(255,255,255,0.4)' }}
          animate={{ x: value ? 22 : 2 }}
          transition={{ type: 'spring', stiffness: 500, damping: 30 }}
        />
      </button>
    </div>
  );
}
