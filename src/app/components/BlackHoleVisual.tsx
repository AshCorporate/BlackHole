import { motion } from 'motion/react';

interface BlackHoleVisualProps {
  glowColor?: string;
  size?: number;
  className?: string;
  intensity?: number;
  animate?: boolean;
}

export function BlackHoleVisual({
  glowColor = '#8b5cf6',
  size = 120,
  className = '',
  intensity = 70,
  animate = true,
}: BlackHoleVisualProps) {
  const gIntensity = intensity / 100;

  return (
    <div
      className={`relative flex items-center justify-center select-none ${className}`}
      style={{ width: size * 2.2, height: size * 2.2 }}
    >
      {/* Outer nebula cloud */}
      <motion.div
        style={{
          position: 'absolute',
          width: size * 2.2,
          height: size * 2.2,
          borderRadius: '50%',
          background: `radial-gradient(circle, ${glowColor}${Math.round(gIntensity * 0.18 * 255).toString(16).padStart(2, '0')} 0%, ${glowColor}08 50%, transparent 70%)`,
        }}
        animate={animate ? { scale: [1, 1.06, 1], opacity: [0.8, 1, 0.8] } : {}}
        transition={{ duration: 3, repeat: Infinity, ease: 'easeInOut' }}
      />

      {/* Gravitational lens ring 1 */}
      <motion.div
        style={{
          position: 'absolute',
          width: size * 1.55,
          height: size * 0.38,
          borderRadius: '50%',
          border: `1.5px solid ${glowColor}55`,
          boxShadow: `0 0 8px ${glowColor}40`,
        }}
        animate={animate ? { rotate: 360 } : {}}
        transition={{ duration: 9, repeat: Infinity, ease: 'linear' }}
      />

      {/* Gravitational lens ring 2 */}
      <motion.div
        style={{
          position: 'absolute',
          width: size * 1.35,
          height: size * 0.3,
          borderRadius: '50%',
          border: `1px solid ${glowColor}70`,
          boxShadow: `0 0 12px ${glowColor}50`,
        }}
        animate={animate ? { rotate: -360 } : {}}
        transition={{ duration: 6, repeat: Infinity, ease: 'linear' }}
      />

      {/* Accretion disk glow */}
      <motion.div
        style={{
          position: 'absolute',
          width: size * 1.7,
          height: size * 0.18,
          borderRadius: '50%',
          background: `radial-gradient(ellipse, ${glowColor}50 0%, ${glowColor}20 50%, transparent 80%)`,
        }}
        animate={animate ? { rotate: [0, 10, -10, 0], opacity: [0.6, 1, 0.6] } : {}}
        transition={{ duration: 5, repeat: Infinity, ease: 'easeInOut' }}
      />

      {/* Spinning energy ring */}
      <motion.div
        style={{
          position: 'absolute',
          width: size * 1.15,
          height: size * 1.15,
          borderRadius: '50%',
          background: `conic-gradient(from 0deg, transparent 0%, ${glowColor}40 25%, transparent 50%, ${glowColor}20 75%, transparent 100%)`,
        }}
        animate={animate ? { rotate: 360 } : {}}
        transition={{ duration: 3, repeat: Infinity, ease: 'linear' }}
      />

      {/* Core */}
      <div
        style={{
          position: 'relative',
          width: size,
          height: size,
          borderRadius: '50%',
          background: `radial-gradient(circle at 35% 30%, #180d30 0%, #0a0510 35%, #000000 70%)`,
          boxShadow: `0 0 ${size * 0.5 * gIntensity}px ${glowColor}80, 0 0 ${size * 0.25 * gIntensity}px ${glowColor}aa, 0 0 ${size * 0.1}px ${glowColor}ff, inset 0 0 ${size * 0.2}px ${glowColor}15`,
          zIndex: 2,
        }}
      >
        {/* Inner swirl */}
        <motion.div
          style={{
            position: 'absolute',
            inset: 0,
            borderRadius: '50%',
            background: `conic-gradient(from 0deg, transparent 0%, ${glowColor}20 20%, transparent 40%, ${glowColor}12 60%, transparent 80%)`,
          }}
          animate={animate ? { rotate: -360 } : {}}
          transition={{ duration: 4, repeat: Infinity, ease: 'linear' }}
        />
        {/* Specular highlight */}
        <div
          style={{
            position: 'absolute',
            top: '18%',
            left: '22%',
            width: '28%',
            height: '22%',
            borderRadius: '50%',
            background: `radial-gradient(circle, ${glowColor}30 0%, transparent 100%)`,
            filter: 'blur(4px)',
          }}
        />
      </div>

      {/* Orbiting particles */}
      {[0, 72, 144, 216, 288].map((deg, i) => (
        <motion.div
          key={i}
          style={{
            position: 'absolute',
            width: size * 1.25,
            height: size * 1.25,
          }}
          animate={animate ? { rotate: 360 } : {}}
          transition={{ duration: 7 + i * 0.8, repeat: Infinity, ease: 'linear', delay: i * 0.3 }}
        >
          <div
            style={{
              position: 'absolute',
              top: '0',
              left: '50%',
              transform: `translateX(-50%) rotate(${deg}deg)`,
              width: i % 2 === 0 ? 3 : 2,
              height: i % 2 === 0 ? 3 : 2,
              borderRadius: '50%',
              background: glowColor,
              boxShadow: `0 0 6px ${glowColor}`,
              opacity: 0.7 + i * 0.05,
            }}
          />
        </motion.div>
      ))}
    </div>
  );
}
