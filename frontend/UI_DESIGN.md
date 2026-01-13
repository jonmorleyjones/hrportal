# UI Design System

## Overview

This application uses a **Glassmorphism** design system with a dark aurora theme and comprehensive Framer Motion animations.

## Design Philosophy

- **Style**: Glassmorphism with frosted glass effects
- **Theme**: Dark aurora with gradient accents
- **Motion**: Smooth, purposeful animations throughout
- **Typography**: Outfit font family for modern, clean readability

## Color System

### CSS Variables (defined in `index.css`)

```css
--background: 222 47% 6%      /* Deep dark blue */
--foreground: 210 40% 98%     /* Near white */
--primary: 217 91% 60%        /* Vibrant blue */
--accent: 280 85% 65%         /* Purple accent */
--muted: 217 33% 17%          /* Muted backgrounds */
--border: 217 33% 20%         /* Subtle borders */
```

### Gradient Accents

- Primary to Accent gradients for buttons and highlights
- Aurora-style animated background gradients
- Floating orb effects with blur and opacity

## Glass Effects

### Utility Classes

| Class | Description |
|-------|-------------|
| `.glass` | Standard glass card with backdrop-blur-xl |
| `.glass-subtle` | Lighter glass effect for nested elements |
| `.glass-strong` | More opaque glass for prominent elements |
| `.gradient-text` | Gradient text effect (primary to accent) |

### Glass Card Properties

```css
.glass {
  background: rgba(255, 255, 255, 0.03);
  backdrop-filter: blur(24px);
  border: 1px solid rgba(255, 255, 255, 0.06);
  box-shadow:
    0 8px 32px rgba(0, 0, 0, 0.3),
    inset 0 1px 0 rgba(255, 255, 255, 0.05);
}
```

## Animation System

### Components (`components/ui/motion.tsx`)

| Component | Purpose |
|-----------|---------|
| `PageTransition` | Wraps page content with fade/slide animation |
| `StaggerContainer` | Parent for staggered children animations |
| `StaggerItem` | Child element with staggered entrance |
| `HoverCard` | Card with lift effect on hover |
| `GlassCard` | Pre-styled glass card with animations |
| `Skeleton` | Shimmer loading placeholder |
| `Spinner` | Rotating loading indicator |
| `GradientBackground` | Animated background with floating orbs |

### Animation Variants

```typescript
fadeIn        // Simple opacity fade
fadeInUp      // Fade + slide up
fadeInDown    // Fade + slide down
slideInLeft   // Slide from left
slideInRight  // Slide from right
scaleIn       // Scale up from 0.9
```

### Stagger Configuration

```typescript
staggerContainer: {
  staggerChildren: 0.1,
  delayChildren: 0.2
}
```

## Page Implementations

### Login Page
- Centered glass card with gradient overlay
- Animated floating decorative elements
- Staggered form field animations
- Gradient submit button with hover effects
- Tenant branding display

### Dashboard
- Staggered stat cards grid (4 columns on desktop)
- Area chart with gradient fill
- Activity feed with timeline animation
- Real-time data updates via React Query

### Users Page
- Collapsible invite form with AnimatePresence
- Animated table with row entrance effects
- Gradient user avatars with initials
- Dropdown menus for actions
- Status badges with indicator dots

### Settings Page
- Organized sections: Organization, Preferences, Branding
- Custom toggle switches with gradient active state
- Color pickers with live preview
- Save buttons with loading/success states

### Billing Page
- Pricing cards with hover lift effects
- "Popular" plan highlight with ring border
- Current plan indicator
- Invoice history table with animations
- Status badges for payment status

## Layout Components

### AppShell
- GradientBackground wrapper
- PageTransition for route changes
- Sidebar + Header + Main content structure

### Sidebar
- Glass-strong styling
- Animated nav items with active indicator
- Subscription badge with usage progress
- Smooth entrance animation

### Header
- Glass rounded design
- Search input with icon
- Notification button with badge
- User dropdown menu

## Tailwind Extensions

### Custom Shadows

```javascript
'glass': '0 8px 32px rgba(0, 0, 0, 0.3)',
'glass-lg': '0 25px 50px -12px rgba(0, 0, 0, 0.5)',
'glow': '0 0 20px rgba(59, 130, 246, 0.5)',
'glow-accent': '0 0 20px rgba(168, 85, 247, 0.5)',
```

### Keyframe Animations

- `fade-in`, `fade-in-up`, `fade-in-down`
- `slide-in-left`, `slide-in-right`
- `scale-in`, `bounce-in`
- `shimmer` (loading effect)
- `float` (subtle vertical movement)
- `glow-pulse` (pulsing glow effect)

## Dependencies

- **framer-motion**: Animation library for React
- **lucide-react**: Icon library
- **recharts**: Chart library (with custom styling)
- **@radix-ui**: Accessible UI primitives (via shadcn/ui)

## File Structure

```
src/
├── index.css                    # Global styles & theme
├── components/
│   ├── ui/
│   │   ├── motion.tsx          # Animation components
│   │   ├── button.tsx          # shadcn button
│   │   └── ...                 # Other UI primitives
│   ├── layout/
│   │   ├── AppShell.tsx        # Main layout wrapper
│   │   ├── Sidebar.tsx         # Navigation sidebar
│   │   └── Header.tsx          # Top header bar
│   └── shared/
│       ├── PageHeader.tsx      # Page title component
│       └── StatCard.tsx        # Dashboard stat card
└── features/
    ├── auth/LoginPage.tsx
    ├── dashboard/DashboardPage.tsx
    ├── users/UsersPage.tsx
    ├── settings/SettingsPage.tsx
    └── billing/BillingPage.tsx
```

## Usage Examples

### Basic Glass Card

```tsx
<motion.div
  whileHover={{ scale: 1.005 }}
  className="glass rounded-xl p-6"
>
  Content here
</motion.div>
```

### Staggered List

```tsx
<StaggerContainer className="space-y-4">
  {items.map(item => (
    <StaggerItem key={item.id}>
      <div className="glass p-4">{item.name}</div>
    </StaggerItem>
  ))}
</StaggerContainer>
```

### Animated Button

```tsx
<motion.div whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }}>
  <Button className="bg-gradient-to-r from-primary to-accent">
    Click Me
  </Button>
</motion.div>
```

### Loading Skeleton

```tsx
{isLoading ? (
  <Skeleton className="h-8 w-32" />
) : (
  <span>{data.value}</span>
)}
```
