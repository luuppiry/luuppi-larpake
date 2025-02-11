# Differences with react and vanilla html, js, css

## Starting

Install node dependencies
```ps
npm i
```

Run
```ps
npm run start
```

## Chages from vanilla

### images need to be imported and used with braces
```js
import image from "../assets/kiasa.png";

<img src={image}>
```

### If importing image fails, try importing

```js
import "../imageTypes.ts";
```

### Import styles

```js
import "../styles/header.css";
```


### Other
- `.tsx` file extension
- meta tags are set with `<Helmet>` in `Header.tsx`