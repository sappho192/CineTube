package com.servlet;

import java.sql.*;
import java.text.SimpleDateFormat;
import java.io.IOException;
import java.io.PrintWriter;

import javax.servlet.ServletException;
import javax.servlet.annotation.WebServlet;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;
import javax.servlet.http.HttpSession;

/**
 * Servlet implementation class Form
 */
@WebServlet("/Login")
public class Login extends HttpServlet {
	private static final long serialVersionUID = 1L;
       
    /**
     * @see HttpServlet#HttpServlet()
     */
    public Login() {
        super();
    }

	/**
	 * @see HttpServlet#doPost(HttpServletRequest request, HttpServletResponse response)
	 */
	protected void doPost(HttpServletRequest request, HttpServletResponse response) throws ServletException, IOException {
			try {
				login(request, response);
			} catch (ClassNotFoundException e) {
				e.printStackTrace();
			} catch (SQLException e) {
				e.printStackTrace();
			}
	}

	protected void login(HttpServletRequest request, HttpServletResponse response)
	throws ServletException, IOException, ClassNotFoundException, SQLException{
		try {
			// mssql 연결
			Class.forName("com.microsoft.sqlserver.jdbc.SQLServerDriver");
			String connectionUrl = "jdbc:sqlserver://localhost:1433;Database=IP_final;IntegratedSecurity=true";
			Connection conn = DriverManager.getConnection(connectionUrl);
			
			// 한글 인코딩
			request.setCharacterEncoding("UTF-8");
			
			// id, pw 받아오기
			String id = request.getParameter("id");
			String pw = request.getParameter("pw");
			
			// Query 진행
			PreparedStatement pstmt = conn.prepareStatement("SELECT pw, u_num FROM user_data WHERE id IN( ? )");
			pstmt.setString(1, id);
			ResultSet rs = pstmt.executeQuery();

			// DB에 해당 ID가 존재하는 경우
			if (rs.next()) {
				// 비밀번호가 동일한 경우
				if (pw.equals(rs.getString("pw"))) {						
					// 넘겨줄 Attribute
					HttpSession session = request.getSession();
					session.setAttribute("u_num", rs.getString("u_num"));
					session.setAttribute("id", id);
					session.setAttribute("logined", "login_complete");
					
					SimpleDateFormat sdt = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
					String cur_time = sdt.format(new Date(System.currentTimeMillis()));
					System.out.println(id + " login " + cur_time);

					// Welcome.jsp로 이동
					response.sendRedirect("Welcome.jsp");
				} else {
					// 알림창 띄우는 용도
					response.setContentType("text/html; charset=UTF-8");
					PrintWriter out = response.getWriter();

					// 경고창 띄우고 첫 화면으로 이동
					out.println("<script>");
					out.println("alert('비밀번호가 올바르지 않습니다.');");
					out.println("location.href = 'Login.jsp'"); 
					out.println("</script>"); 
					out.close();
				}
			}
			else {
				// 알림창 띄우는 용도
				response.setContentType("text/html; charset=UTF-8");
				PrintWriter out = response.getWriter();

				// 경고창 띄우고 첫 화면으로 이동
				out.println("<script>");
				out.println("alert('존재하지 않는 ID 입니다.');");
				out.println("location.href = 'Login.jsp'"); 
				out.println("</script>"); 
				out.close();
			}

			rs.close();
			pstmt.close();
			conn.close();
		} catch (SQLException sqle) {
			System.out.println("SQLException : " + sqle);
		}
	}
}
