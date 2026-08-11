# Hướng dẫn S0-02 — Docker Compose: PostgreSQL + Redis + pgAdmin

> Dành cho người **lần đầu dùng Docker**. Mỗi khái niệm và mỗi dòng cấu hình đều có giải thích.
> Chạy trong **PowerShell**, tại `D:\Ksyto\Booking-app`.
>
> **Mục tiêu (NFR-39):** máy trắng chỉ cần **một lệnh** là có đủ hạ tầng để chạy dự án.

---

## 0. Sáu khái niệm phải hiểu trước khi gõ lệnh

### 0.1 Vì sao không cài PostgreSQL thẳng lên máy?

| Cài trực tiếp | Dùng Docker |
|---|---|
| Máy bạn PostgreSQL 16, máy đồng nghiệp 14 → lỗi khác nhau | Ai cũng chạy **đúng một phiên bản** |
| Muốn thử PostgreSQL 17 phải gỡ cài đặt | Đổi một dòng, chạy song song được |
| Gỡ bỏ để lại rác trong registry, service | `docker compose down -v` là sạch trơn |
| Môi trường dev khác production | **Giống production** vì cùng image |

> Đây là ý nghĩa thật của Docker trong dự án này: **tái lập được**. Không phải "cho hiện đại".

### 0.2 Image và Container

| | Là gì | Ví dụ đời thường |
|---|---|---|
| **Image** | Bản đóng gói **bất biến**: hệ điều hành tối giản + phần mềm + cấu hình mặc định | Bản thiết kế ngôi nhà |
| **Container** | Một **tiến trình đang chạy** dựng từ image | Ngôi nhà đã xây |

Từ **một** image dựng được **nhiều** container. Xoá container không mất image.

### 0.3 Docker Compose

Một file YAML mô tả **nhiều container chạy cùng nhau** và cách chúng nối với nhau. Thay vì gõ 3 lệnh `docker run` dài dằng dặc, bạn viết một lần rồi `docker compose up`.

### 0.4 Volume — 🔴 khái niệm quan trọng nhất

**Container không lưu trạng thái.** Xoá container → **mất sạch dữ liệu bên trong**.

**Volume** là vùng lưu trữ nằm **ngoài** container, do Docker quản lý. Container chết, volume còn.

```
postgres-data (volume)  ←──  gắn vào  ──→  /var/lib/postgresql/data (trong container)
```

> ⚠️ Không khai báo volume = mỗi lần `docker compose down` là mất toàn bộ CSDL. Đây là lỗi kinh điển của người mới.

### 0.5 Port mapping

```
"5432:5432"
 │     └── cổng BÊN TRONG container
 └──────── cổng TRÊN MÁY BẠN
```

Không có dòng này thì ứng dụng .NET trên máy bạn **không nối được** vào PostgreSQL trong container.

### 0.6 Healthcheck

Container **đang chạy** ≠ dịch vụ **đã sẵn sàng**. PostgreSQL mất 3–10 giây để khởi động xong.

`healthcheck` dạy Docker cách kiểm tra thật. Kết hợp với `depends_on: condition: service_healthy` để container khác **chờ đúng lúc**.

> Không có healthcheck, API sẽ khởi động rồi lỗi ngay vì CSDL chưa sẵn sàng. Sau này CI cũng dựa vào đúng cơ chế này.

---

## 1. Kiểm tra Docker

```powershell
docker --version
```

```powershell
docker compose version
```

```powershell
docker ps
```

Lệnh thứ ba phải chạy được *(dù danh sách rỗng)*. Nếu báo lỗi kết nối → **Docker Desktop chưa bật**. Mở nó lên và chờ biểu tượng chuyển sang xanh.

---

## 2. Tạo file bí mật

### 2.1 `.env.example` — mẫu, **được** commit

Tạo file `.env.example` ở thư mục gốc:

```dotenv
# ---- PostgreSQL ----
POSTGRES_DB=courtbooking
POSTGRES_USER=courtbooking
POSTGRES_PASSWORD=doi-mat-khau-nay
POSTGRES_PORT=5432

# ---- Redis ----
REDIS_PASSWORD=doi-mat-khau-nay
REDIS_PORT=6379

# ---- pgAdmin (chỉ môi trường dev) ----
PGADMIN_EMAIL=admin@courtbooking.local
PGADMIN_PASSWORD=doi-mat-khau-nay
PGADMIN_PORT=5050
```

File này **liệt kê tên biến** cần có, với giá trị giả. Người mới vào dự án nhìn vào là biết phải cấu hình gì.

### 2.2 `.env` — thật, **KHÔNG** commit

```powershell
Copy-Item .env.example .env
```

Mở `.env`, đổi **cả ba** mật khẩu thành chuỗi thật.

**Kiểm tra `.env` đã bị chặn khỏi git:**

```powershell
git check-ignore -v .env
```

Phải in ra dòng khớp trong `.gitignore`. **Nếu không in ra gì → dừng lại**, `.env` sẽ bị commit lên GitHub.

> 🔑 Đây chính là mẫu bạn đã học ở phần `.gitignore`: `.env.*` chặn tất cả, `!.env.example` mở ngoại lệ cho file mẫu.

---

## 3. Viết `docker-compose.yml`

Tạo file `docker-compose.yml` ở thư mục gốc:

```yaml
services:

  postgres:
    image: postgres:16-alpine
    container_name: courtbooking-postgres
    restart: unless-stopped
    environment:
      POSTGRES_DB: ${POSTGRES_DB}
      POSTGRES_USER: ${POSTGRES_USER}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
      TZ: UTC
    ports:
      - "${POSTGRES_PORT:-5432}:5432"
    volumes:
      - postgres-data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER} -d ${POSTGRES_DB}"]
      interval: 10s
      timeout: 5s
      retries: 5
      start_period: 10s

  redis:
    image: redis:7-alpine
    container_name: courtbooking-redis
    restart: unless-stopped
    command: ["redis-server", "--appendonly", "yes", "--requirepass", "${REDIS_PASSWORD}"]
    ports:
      - "${REDIS_PORT:-6379}:6379"
    volumes:
      - redis-data:/data
    healthcheck:
      test: ["CMD", "redis-cli", "--no-auth-warning", "-a", "${REDIS_PASSWORD}", "ping"]
      interval: 10s
      timeout: 3s
      retries: 5

  pgadmin:
    image: dpage/pgadmin4:8.14
    container_name: courtbooking-pgadmin
    profiles: ["tools"]
    restart: unless-stopped
    environment:
      PGADMIN_DEFAULT_EMAIL: ${PGADMIN_EMAIL}
      PGADMIN_DEFAULT_PASSWORD: ${PGADMIN_PASSWORD}
      PGADMIN_CONFIG_SERVER_MODE: "False"
    ports:
      - "${PGADMIN_PORT:-5050}:80"
    volumes:
      - pgadmin-data:/var/lib/pgadmin
    depends_on:
      postgres:
        condition: service_healthy

volumes:
  postgres-data:
  redis-data:
  pgadmin-data:
```

### Giải thích từng quyết định

| Dòng | Vì sao |
|---|---|
| **Không có `version:`** | Khoá `version` đã lỗi thời từ Compose v2. Viết vào sẽ bị cảnh báo |
| `postgres:16-alpine` | **Ghim phiên bản.** Không bao giờ dùng `:latest` — build hôm nay và tháng sau phải cho kết quả giống nhau |
| `-alpine` | Bản trên nền Alpine Linux, nhẹ hơn ~5 lần. Tải nhanh, ít bề mặt tấn công |
| `restart: unless-stopped` | Container tự bật lại khi máy khởi động lại. Không tự bật nếu **bạn** chủ động dừng nó |
| `TZ: UTC` | Khớp quy ước dự án — **CSDL luôn lưu UTC**. Đặt ở đây để không phụ thuộc múi giờ máy dev |
| `${POSTGRES_PORT:-5432}` | Lấy từ `.env`, **mặc định 5432** nếu không có. Cú pháp `:-` là giá trị dự phòng |
| `volumes: postgres-data:` | **Named volume** — dữ liệu sống sót qua `docker compose down` |
| `pg_isready` | Lệnh chuẩn của PostgreSQL để hỏi "đã nhận kết nối chưa?" |
| `start_period: 10s` | 10 giây đầu, healthcheck fail **không bị tính** — cho PostgreSQL thời gian khởi động |
| `--appendonly yes` | Redis ghi dữ liệu xuống đĩa. Bật để test hành vi cache sau khi restart |
| `--requirepass` | **Đặt mật khẩu Redis ngay từ dev.** Redis mặc định mở toang — nhiều vụ rò rỉ dữ liệu bắt nguồn từ đây |
| `profiles: ["tools"]` | pgAdmin **không** khởi động mặc định. Xem giải thích bên dưới |
| `depends_on: service_healthy` | pgAdmin chờ PostgreSQL **thật sự sẵn sàng**, không chỉ "đã chạy" |

### 🔑 Vì sao pgAdmin nằm trong `profiles`?

pgAdmin là **công cụ cho lập trình viên**, không phải thành phần của hệ thống. Nếu để nó khởi động mặc định:

- CI sẽ tốn thời gian kéo và chạy một container vô ích
- Người khác đọc file sẽ tưởng nó là phần của kiến trúc
- Rủi ro ai đó bê nguyên file này lên production

`profiles` tách rõ **thành phần hệ thống** và **công cụ hỗ trợ**. Đây là thói quen nên có từ đầu.

---

## 4. Khởi động

```powershell
docker compose up -d
```

`-d` = *detached*, chạy nền và trả lại con trỏ. Không có `-d` thì log đổ thẳng ra màn hình và `Ctrl+C` sẽ tắt luôn container.

Lần đầu sẽ mất 1–3 phút để tải image.

**Kiểm tra:**

```powershell
docker compose ps
```

Cả `postgres` và `redis` phải ở trạng thái `Up` kèm `(healthy)`. Nếu thấy `(health: starting)` thì chờ ~15 giây rồi chạy lại.

Muốn dùng pgAdmin:

```powershell
docker compose --profile tools up -d
```

---

## 5. Nghiệm thu từng dịch vụ

### 5.1 PostgreSQL

```powershell
docker compose exec postgres psql -U courtbooking -d courtbooking -c "SELECT version();"
```

`docker compose exec <service> <lệnh>` = chạy lệnh **bên trong** container đang chạy.

Phải in ra `PostgreSQL 16.x ...`.

Kiểm tra múi giờ đúng UTC:

```powershell
docker compose exec postgres psql -U courtbooking -d courtbooking -c "SHOW timezone;"
```

### 5.2 Redis

```powershell
docker compose exec redis redis-cli -a "$((Get-Content .env | Select-String '^REDIS_PASSWORD=').Line.Split('=')[1])" ping
```

Phải trả về `PONG`.

*(Nếu lệnh trên khó nhớ, cách đơn giản hơn: `docker compose exec redis redis-cli` rồi gõ `AUTH <mật-khẩu>` và `PING`.)*

### 5.3 pgAdmin

Mở trình duyệt: **http://localhost:5050**

Đăng nhập bằng `PGADMIN_EMAIL` / `PGADMIN_PASSWORD` trong `.env`.

Thêm server mới với thông số:

| Trường | Giá trị | ⚠️ Lưu ý |
|---|---|---|
| Host | **`postgres`** | **Không phải `localhost`!** Đây là tên service trong compose |
| Port | `5432` | Cổng **bên trong** mạng Docker, không phải cổng đã map ra ngoài |
| Database / Username | `courtbooking` | |
| Password | `POSTGRES_PASSWORD` | |

> 🔑 **Điểm này người mới hay vấp:** container nói chuyện với nhau qua **mạng nội bộ của Docker**, ở đó mỗi service có tên riêng làm hostname. `localhost` bên trong container pgAdmin trỏ về **chính pgAdmin**, không phải máy bạn.
>
> Ứng dụng .NET chạy **trên máy bạn** thì ngược lại — dùng `localhost:5432`.

---

## 6. Lệnh dùng hằng ngày

| Việc | Lệnh |
|---|---|
| Bật | `docker compose up -d` |
| Tắt *(giữ dữ liệu)* | `docker compose down` |
| Xem trạng thái | `docker compose ps` |
| Xem log | `docker compose logs -f postgres` |
| Vào shell container | `docker compose exec postgres sh` |
| Khởi động lại một service | `docker compose restart redis` |
| 🔴 **Xoá sạch kể cả dữ liệu** | `docker compose down -v` |

> ⚠️ **`down -v` xoá toàn bộ volume.** Toàn bộ CSDL biến mất, không hoàn tác được. Chỉ dùng khi muốn làm lại từ đầu.

---

## 7. Lỗi thường gặp

| Lỗi | Nguyên nhân | Cách sửa |
|---|---|---|
| `Cannot connect to the Docker daemon` | Docker Desktop chưa chạy | Mở Docker Desktop, chờ icon xanh |
| `port is already allocated` | Máy đã có PostgreSQL/Redis chiếm cổng | Đổi `POSTGRES_PORT=5433` trong `.env` rồi `docker compose up -d` lại |
| Container `unhealthy` | Sai mật khẩu, hoặc chưa khởi động xong | `docker compose logs postgres` đọc nguyên nhân thật |
| `variable is not set` | Chưa tạo `.env`, hoặc thiếu biến | Đối chiếu lại với `.env.example` |
| pgAdmin không nối được | Điền `localhost` thay vì `postgres` | Dùng **tên service** làm host |
| Đổi `.env` mà không có tác dụng | Container cũ vẫn giữ biến cũ | `docker compose up -d --force-recreate` |
| Dữ liệu mất sau khi `down` | Đã lỡ chạy `down -v` | Không cứu được — đây là lý do volume quan trọng |

---

## ✅ Definition of Done

- [x] `.env.example` có, đã commit
- [x] `.env` có trên máy, **`git check-ignore .env` trả về kết quả** *(bị chặn)*
- [x] `docker compose up -d` → `postgres` và `redis` đều `Up (healthy)`
- [x] `psql ... SELECT version();` trả về PostgreSQL 16.x
- [x] `SHOW timezone;` trả về `UTC`
- [x] Redis trả `PONG` **và yêu cầu mật khẩu**
- [x] pgAdmin *(profile `tools`)* nối được vào PostgreSQL bằng host `postgres`
- [x] `docker compose down` rồi `up -d` lại → **dữ liệu còn nguyên**
- [x] Không có mật khẩu thật nào trong `docker-compose.yml` hay `.env.example`

### 🧪 Bài kiểm tra quan trọng nhất — chứng minh volume hoạt động

```powershell
docker compose exec postgres psql -U courtbooking -d courtbooking -c "CREATE TABLE test_volume(id int); INSERT INTO test_volume VALUES (42);"
```

```powershell
docker compose down
```

```powershell
docker compose up -d
```

```powershell
docker compose exec postgres psql -U courtbooking -d courtbooking -c "SELECT * FROM test_volume;"
```

Phải vẫn thấy `42`. Nếu bảng biến mất → volume khai báo sai, sửa trước khi đi tiếp.

Dọn bảng test:

```powershell
docker compose exec postgres psql -U courtbooking -d courtbooking -c "DROP TABLE test_volume;"
```

---

## 📚 Sáu thứ vừa học — sẽ bị hỏi khi phỏng vấn

1. **Vì sao dùng Docker cho CSDL thay vì cài trực tiếp?**
   → Tái lập được, cách ly phiên bản, giống production, dọn sạch dễ.

2. **Volume để làm gì? Không có thì sao?**
   → Container không lưu trạng thái. Không có volume thì `down` là mất toàn bộ dữ liệu.

3. **Vì sao ghim phiên bản image thay vì `:latest`?**
   → Build phải tái lập được. `:latest` khiến hôm nay và tháng sau ra hai kết quả khác nhau — và lỗi đó cực khó truy nguyên.

4. **`depends_on` có đủ để đảm bảo CSDL sẵn sàng không?**
   → **Không.** `depends_on` chỉ chờ container *khởi động*, không chờ dịch vụ *sẵn sàng*. Phải kèm `healthcheck` + `condition: service_healthy`.

5. **Vì sao container này gọi container kia bằng tên service, không phải `localhost`?**
   → Compose tạo mạng nội bộ, mỗi service là một hostname. `localhost` trong container trỏ về chính container đó.

6. **Vì sao đặt mật khẩu cho Redis ngay từ môi trường dev?**
   → Redis mặc định không xác thực. Tập thói quen bật từ đầu, vì cấu hình dev hay bị bê nguyên lên staging.

---

## ➡️ Task tiếp theo

**S0-03 — EF Core `DbContext` + migration đầu tiên** *(`tenant`, `branch`, `court`)*.
Đây là lần đầu code C# thật của dự án — và là lúc [07-domain-model.md](../07-domain-model.md) được đem ra dùng.
