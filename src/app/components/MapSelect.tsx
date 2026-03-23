import { useNavigate } from 'react-router';
import { motion } from 'motion/react';
import { useGame, MAPS } from '../context/GameContext';

const DIFFICULTY_COLORS = {
  Easy: '#22c55e',
  Medium: '#eab308',
  Hard: '#ef4444',
};

const MAP_OBJECTS: Record<string, string[]> = {
  city: ['🏢', '🚗', '🚕', '🏪', '🌳', '🚌'],
  space: ['🛸', '🌠', '☄️', '🛰️', '🌌', '🔭'],
  desert: ['🌵', '🐪', '🏜️', '🌊', '🦂', '☀️'],
  ocean: ['🐋', '🦈', '🐙', '🐠', '🦀', '🌊'],
  arctic: ['🐧', '🦭', '❄️', '🏔️', '🐻‍❄️', '🌨️'],
  volcano: ['🌋', '🔥', '💎', '🦖', '🪨', '⚡'],
};

export function MapSelect() {
  const navigate = useNavigate();
  const game = useGame();

  return (
    <div className="min-h-screen flex flex-col px-4 pt-6 pb-10 max-w-2xl mx-auto">

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
          ВЫБОР КАРТЫ
        </h2>

        <div style={{ width: 80 }} />
      </motion.div>

      {/* Current selection label */}
      <motion.div
        className="mb-5 text-center"
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ delay: 0.15 }}
      >
        <span style={{ color: 'rgba(255,255,255,0.4)', fontSize: '0.8rem', fontFamily: "'Rajdhani', sans-serif" }}>
          Выбрана карта: &nbsp;
        </span>
        <span style={{ color: game.glowColor, fontSize: '0.9rem', fontFamily: "'Orbitron', monospace", fontWeight: 700 }}>
          {MAPS.find(m => m.id === game.selectedMap)?.name}
        </span>
      </motion.div>

      {/* Maps grid */}
      <div className="grid grid-cols-2 gap-4">
        {MAPS.map((map, i) => {
          const selected = game.selectedMap === map.id;
          const diffColor = DIFFICULTY_COLORS[map.difficulty];

          return (
            <motion.button
              key={map.id}
              className="relative flex flex-col rounded-2xl overflow-hidden text-left"
              style={{
                background: selected
                  ? `linear-gradient(135deg, ${game.glowColor}20, rgba(0,0,0,0.6))`
                  : `linear-gradient(135deg, ${map.bgFrom}cc, ${map.bgTo}cc)`,
                border: selected
                  ? `2px solid ${game.glowColor}80`
                  : '1px solid rgba(255,255,255,0.08)',
                boxShadow: selected ? `0 0 24px ${game.glowColor}30` : 'none',
              }}
              initial={{ opacity: 0, scale: 0.9 }}
              animate={{ opacity: 1, scale: 1 }}
              transition={{ delay: i * 0.07 }}
              whileHover={{ scale: 1.03 }}
              whileTap={{ scale: 0.97 }}
              onClick={() => game.selectMap(map.id)}
            >
              {/* Map visual area */}
              <div
                className="w-full flex items-center justify-center py-5"
                style={{
                  background: `linear-gradient(135deg, ${map.bgFrom}, ${map.bgTo})`,
                  position: 'relative',
                  overflow: 'hidden',
                }}
              >
                {/* Background objects */}
                <div className="absolute inset-0 flex flex-wrap items-center justify-center gap-2 opacity-20 pointer-events-none px-2">
                  {MAP_OBJECTS[map.id]?.map((obj, j) => (
                    <span key={j} style={{ fontSize: '1.4rem' }}>{obj}</span>
                  ))}
                </div>
                {/* Main emoji */}
                <div style={{ fontSize: '3.5rem', position: 'relative', zIndex: 1 }}>{map.emoji}</div>

                {/* Selected overlay */}
                {selected && (
                  <motion.div
                    className="absolute inset-0 flex items-center justify-center"
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                  >
                    <div
                      className="w-10 h-10 rounded-full flex items-center justify-center font-bold text-lg"
                      style={{
                        background: game.glowColor,
                        boxShadow: `0 0 20px ${game.glowColor}`,
                        color: 'white',
                      }}
                    >
                      ✓
                    </div>
                  </motion.div>
                )}
              </div>

              {/* Info area */}
              <div className="p-3 flex flex-col gap-1">
                <div style={{ fontFamily: "'Orbitron', monospace", fontWeight: 700, color: 'white', fontSize: '0.75rem' }}>
                  {map.name.toUpperCase()}
                </div>
                <div style={{ color: 'rgba(255,255,255,0.5)', fontSize: '0.72rem', lineHeight: 1.3 }}>
                  {map.description}
                </div>
                <div className="flex items-center gap-2 mt-1">
                  <div
                    className="px-2 py-0.5 rounded-lg text-xs font-bold"
                    style={{
                      background: `${diffColor}20`,
                      border: `1px solid ${diffColor}50`,
                      color: diffColor,
                      fontFamily: "'Orbitron', monospace",
                      fontSize: '0.55rem',
                    }}
                  >
                    {map.difficulty === 'Easy' ? 'ЛЕГКО' : map.difficulty === 'Medium' ? 'СРЕДНЕ' : 'СЛОЖНО'}
                  </div>
                  {selected && (
                    <div
                      className="px-2 py-0.5 rounded-lg text-xs font-bold"
                      style={{
                        background: `${game.glowColor}20`,
                        border: `1px solid ${game.glowColor}50`,
                        color: game.glowColor,
                        fontFamily: "'Orbitron', monospace",
                        fontSize: '0.55rem',
                      }}
                    >
                      ✓ ВЫБРАНО
                    </div>
                  )}
                </div>
              </div>
            </motion.button>
          );
        })}
      </div>

      {/* Start button */}
      <motion.div
        className="mt-6"
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.5 }}
      >
        <button
          className="w-full py-4 rounded-2xl font-black tracking-widest uppercase"
          style={{
            fontFamily: "'Orbitron', monospace",
            fontSize: '0.9rem',
            color: 'white',
            background: `linear-gradient(135deg, ${game.glowColor}cc, ${game.glowColor}88)`,
            border: `1.5px solid ${game.glowColor}`,
            boxShadow: `0 0 24px ${game.glowColor}60`,
          }}
          onClick={() => navigate('/')}
        >
          ✓ Подтвердить выбор
        </button>
      </motion.div>
    </div>
  );
}
