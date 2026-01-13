import type { ITheme } from 'survey-core';

export const auroraGlassTheme: ITheme = {
  cssVariables: {
    // Background colors (matching --background: 224 71% 4%)
    '--sjs-general-backcolor': 'transparent',
    '--sjs-general-backcolor-dark': 'hsl(224 71% 4%)',
    '--sjs-general-backcolor-dim': 'hsl(224 71% 8% / 0.7)',
    '--sjs-general-backcolor-dim-light': 'hsl(217 33% 12%)',

    // Foreground colors (matching --foreground: 213 31% 91%)
    '--sjs-general-forecolor': 'hsl(213 31% 91%)',
    '--sjs-general-forecolor-light': 'hsl(215 20% 65%)',
    '--sjs-general-dim-forecolor': 'hsl(215 20% 55%)',
    '--sjs-general-dim-forecolor-light': 'hsl(215 20% 45%)',

    // Primary colors (matching --primary: 262 83% 58%)
    '--sjs-primary-backcolor': 'hsl(262 83% 58%)',
    '--sjs-primary-backcolor-light': 'hsl(262 83% 58% / 0.15)',
    '--sjs-primary-backcolor-dark': 'hsl(262 83% 48%)',
    '--sjs-primary-forecolor': 'hsl(210 40% 98%)',
    '--sjs-primary-forecolor-light': 'hsl(210 40% 98% / 0.8)',

    // Secondary (matching --accent: 199 89% 48%)
    '--sjs-secondary-backcolor': 'hsl(217 33% 17%)',
    '--sjs-secondary-backcolor-light': 'hsl(217 33% 22%)',
    '--sjs-secondary-backcolor-semi-light': 'hsl(217 33% 20%)',
    '--sjs-secondary-forecolor': 'hsl(213 31% 91%)',
    '--sjs-secondary-forecolor-light': 'hsl(215 20% 65%)',

    // Border colors (matching --border: 217 33% 17%)
    '--sjs-border-default': 'hsl(217 33% 25% / 0.5)',
    '--sjs-border-light': 'hsl(217 33% 30% / 0.3)',
    '--sjs-border-inside': 'hsl(217 33% 25% / 0.3)',

    // Shadow
    '--sjs-shadow-small': '0 4px 16px 0 rgba(0, 0, 0, 0.25)',
    '--sjs-shadow-small-reset': 'none',
    '--sjs-shadow-medium': '0 8px 32px 0 rgba(0, 0, 0, 0.37)',
    '--sjs-shadow-large': '0 25px 50px -12px rgba(0, 0, 0, 0.5)',
    '--sjs-shadow-inner': 'inset 0 1px 0 0 hsl(0 0% 100% / 0.05)',
    '--sjs-shadow-inner-reset': 'none',

    // Special
    '--sjs-special-red': 'hsl(0 84% 60%)',
    '--sjs-special-red-light': 'hsl(0 84% 60% / 0.15)',
    '--sjs-special-red-forecolor': 'hsl(210 40% 98%)',
    '--sjs-special-green': 'hsl(160 84% 39%)',
    '--sjs-special-green-light': 'hsl(160 84% 39% / 0.15)',
    '--sjs-special-green-forecolor': 'hsl(210 40% 98%)',
    '--sjs-special-blue': 'hsl(199 89% 48%)',
    '--sjs-special-blue-light': 'hsl(199 89% 48% / 0.15)',
    '--sjs-special-blue-forecolor': 'hsl(210 40% 98%)',
    '--sjs-special-yellow': 'hsl(45 93% 47%)',
    '--sjs-special-yellow-light': 'hsl(45 93% 47% / 0.15)',
    '--sjs-special-yellow-forecolor': 'hsl(224 71% 4%)',

    // Font
    '--sjs-font-family': "'Outfit', system-ui, sans-serif",
    '--sjs-font-size': '16px',
    '--sjs-article-font-large-textDecoration': 'none',
    '--sjs-article-font-large-fontWeight': '600',
    '--sjs-article-font-large-fontStyle': 'normal',
    '--sjs-article-font-large-fontStretch': 'normal',
    '--sjs-article-font-large-letterSpacing': '0',
    '--sjs-article-font-large-lineHeight': '1.4',
    '--sjs-article-font-large-paragraphIndent': '0px',
    '--sjs-article-font-large-textCase': 'none',

    // Border radius
    '--sjs-corner-radius': '0.75rem',
    '--sjs-base-unit': '8px',

    // Editor
    '--sjs-editor-background': 'hsl(224 71% 6% / 0.5)',
    '--sjs-editorpanel-backcolor': 'hsl(224 71% 8% / 0.7)',
    '--sjs-editorpanel-hovercolor': 'hsl(224 71% 10%)',
    '--sjs-editorpanel-cornerRadius': '0.75rem',

    // Question
    '--sjs-questionpanel-backcolor': 'transparent',
    '--sjs-questionpanel-hovercolor': 'hsl(262 83% 58% / 0.05)',
    '--sjs-questionpanel-cornerRadius': '0.75rem',
  },
  themeName: 'aurora-glass',
  colorPalette: 'dark',
  isPanelless: false,
};

export default auroraGlassTheme;
