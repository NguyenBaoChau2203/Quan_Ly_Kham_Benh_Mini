---
name: Clinical Precision
colors:
  surface: '#f8f9ff'
  surface-dim: '#d9dae0'
  surface-bright: '#f8f9ff'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#f2f3f9'
  surface-container: '#ededf4'
  surface-container-high: '#e7e8ee'
  surface-container-highest: '#e1e2e8'
  on-surface: '#191c20'
  on-surface-variant: '#414750'
  inverse-surface: '#2e3035'
  inverse-on-surface: '#eff0f6'
  outline: '#727781'
  outline-variant: '#c1c7d2'
  surface-tint: '#1b60a2'
  primary: '#003e6f'
  on-primary: '#ffffff'
  primary-container: '#005596'
  on-primary-container: '#a4caff'
  inverse-primary: '#a2c9ff'
  secondary: '#505f76'
  on-secondary: '#ffffff'
  secondary-container: '#d0e1fb'
  on-secondary-container: '#54647a'
  tertiary: '#642d00'
  on-tertiary: '#ffffff'
  tertiary-container: '#873f01'
  on-tertiary-container: '#ffb88c'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#d3e4ff'
  primary-fixed-dim: '#a2c9ff'
  on-primary-fixed: '#001c38'
  on-primary-fixed-variant: '#004881'
  secondary-fixed: '#d3e4fe'
  secondary-fixed-dim: '#b7c8e1'
  on-secondary-fixed: '#0b1c30'
  on-secondary-fixed-variant: '#38485d'
  tertiary-fixed: '#ffdbc8'
  tertiary-fixed-dim: '#ffb68a'
  on-tertiary-fixed: '#321300'
  on-tertiary-fixed-variant: '#743500'
  background: '#f8f9ff'
  on-background: '#191c20'
  surface-variant: '#e1e2e8'
typography:
  screen-header:
    fontFamily: Segoe UI
    fontSize: 16px
    fontWeight: '700'
    lineHeight: 24px
  section-header:
    fontFamily: Segoe UI
    fontSize: 14px
    fontWeight: '700'
    lineHeight: 20px
  body-default:
    fontFamily: Segoe UI
    fontSize: 12px
    fontWeight: '400'
    lineHeight: 18px
  grid-cell:
    fontFamily: Segoe UI
    fontSize: 12px
    fontWeight: '400'
    lineHeight: 16px
  label-caps:
    fontFamily: Segoe UI
    fontSize: 11px
    fontWeight: '700'
    lineHeight: 14px
spacing:
  rail-collapsed: 50px
  rail-expanded: 220px
  container-padding: 16px
  element-gap: 8px
  grid-row-height: 28px
  input-height: 32px
---

## Brand & Style
This design system is built for the high-throughput environment of a medical clinic. The personality is **Professional, Systematic, and Calm**, prioritizing data density and legibility over decorative elements. 

The style is **Corporate / Modern**, specifically adapted for the constraints and strengths of the WinForms framework. It utilizes a flat aesthetic with clear structural divisions (Panels and GroupBoxes) to organize complex patient data. By avoiding heavy gradients and shadows, the UI maintains high performance and reduces visual fatigue for medical staff during long shifts.

## Colors
The palette is anchored by **Doctor Blue (#005596)**, evoking trust and authority. The background uses a cool slate-tinted white to reduce screen glare. 

Functional colors are mapped to clinical workflows:
- **Primary:** Actions and active navigation states.
- **Zebra Stripe:** Alternating row color specifically for high-density DataGridViews to maintain horizontal tracking.
- **Status Indicators:** 
    - *Đang chờ (Pending):* Calm blue for steady queues.
    - *Đang khám (In-progress):* Amber to denote active attention.
    - *Đã khám (Completed):* Green for successful resolution.
    - *Đã hủy (Cancelled):* Muted gray to de-emphasize inactive records.

## Typography
The design system utilizes **Segoe UI**, the native Windows typeface, ensuring maximum rendering clarity and system compatibility. 

Hierarchy is established through weight and size rather than color shifts. Screen headers (16px Bold) provide immediate context for the current module, while Section headers (14px Bold) are used within GroupBoxes to categorize form fields. The 12px default size is optimized for high-density data entry, balancing space efficiency with legibility.

## Layout & Spacing
The layout follows a **Fluid Grid** approach optimized for a 1366x768 baseline. It relies heavily on WinForms' **Dock** and **Anchor** properties to ensure the interface scales gracefully.

- **Navigation Rail:** A collapsible left-side panel. When collapsed (50px), it shows icons only; when expanded (220px), it reveals text labels.
- **Main Container:** Uses a 16px padding from the window edges.
- **Grouping:** Content is organized into Panels and GroupBoxes with an 8px internal margin between child controls.
- **Density:** Rows in data grids and lists use a compact 28px height to maximize the number of visible records without scrolling.

## Elevation & Depth
This design system uses **Low-contrast outlines** and **Tonal layers** rather than shadows. 

Depth is communicated through the nesting of controls:
- **Level 0 (Background):** #F8FAFC.
- **Level 1 (Panels/GroupBoxes):** White (#FFFFFF) surfaces with a 1px solid border (#CBD5E1).
- **Level 2 (Inputs/Buttons):** These sit "inside" the Level 1 surfaces, defined by their own borders.

GroupBoxes should use a flat style with the header text integrated into the top border or positioned directly above the frame to maintain a clean, modern silhouette.

## Shapes
To align with the "standard WinForms look" and the efficiency required of an enterprise medical tool, the design system utilizes **Sharp (0px)** roundedness. All panels, buttons, input fields, and grid headers use right-angled corners. This maximizes pixel-perfect alignment and maintains a rigorous, clinical aesthetic.

## Components
- **DataGridView:** Must use `EnableHeadersVisualStyles = false`. Column headers should be #005596 background with white text. Use #F0F5FF for `AlternatingRowsDefaultCellStyle.BackColor`.
- **Buttons:** 
  - *Primary:* Background #005596, ForeColor White, FlatStyle = Flat.
  - *Secondary:* Background White, Border #005596, ForeColor #005596.
- **Input Fields:** 1px solid border #CBD5E1. Use a colored bottom-border (2px) of #005596 only on `Enter` (Focus) events to signal activity.
- **Status Chips:** Small, non-interactive panels within grid cells or headers. Use a solid background of the status color with white text, or a light tinted background with dark text for a softer look.
- **Left Rail Navigation:** Use a `Panel` or `UserControl`. Buttons within should be `FlatStyle.Flat` with `FlatAppearance.BorderSize = 0`. Use high-contrast icons (e.g., FontAwesome or Segoe MDL2 Assets).
- **GroupBox:** Custom paint or standard flat style to ensure the border color is #CBD5E1 and the title is 14px Segoe UI Bold.